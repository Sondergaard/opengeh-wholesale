﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;
using Energinet.DataHub.Wholesale.Infrastructure.BasisData;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Infrastructure.BasisData;

[UnitTest]
public class BatchFileManagerTests
{
    [Fact]
    public async Task GetResultFileStreamAsync_WhenDirectoryDoesNotExist_ThrowsException()
    {
        // Arrange
        var webFileZipperMock = new Mock<IWebFilesZipper>();
        var response = new Mock<Response<bool>>();
        var dataLakeFileSystemClientMock = new Mock<DataLakeFileSystemClient>();
        var dataLakeDirectoryClientMock = new Mock<DataLakeDirectoryClient>();

        response.Setup(res => res.Value).Returns(false);

        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);

        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(response.Object);

        var sut = new BatchFileManager(dataLakeFileSystemClientMock.Object, webFileZipperMock.Object);

        // Act and Assert
        await sut
            .Invoking(s => s.GetResultFileStreamAsync(Guid.NewGuid(), new GridAreaCode("123")))
            .Should()
            .ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetResultFileStreamAsync_WhenNoFilesMatchExtension_ThrowsException()
    {
        // Arrange
        var webFileZipperMock = new Mock<IWebFilesZipper>();
        var response = new Mock<Response<bool>>();
        var dataLakeFileSystemClientMock = new Mock<DataLakeFileSystemClient>();
        var dataLakeDirectoryClientMock = new Mock<DataLakeDirectoryClient>();

        response.Setup(res => res.Value).Returns(true);

        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);

        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(response.Object);

        var sut = new BatchFileManager(dataLakeFileSystemClientMock.Object, webFileZipperMock.Object);

        // Act and Assert
        await sut
            .Invoking(s => s.GetResultFileStreamAsync(Guid.NewGuid(), new GridAreaCode("123")))
            .Should()
            .ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetResultFileStreamAsync_WhenFileExtensionNotFound_ThrowException()
    {
        // Arrange
        var webFileZipperMock = new Mock<IWebFilesZipper>();
        var response = new Mock<Response<bool>>();
        var dataLakeFileSystemClientMock = new Mock<DataLakeFileSystemClient>();
        var dataLakeDirectoryClientMock = new Mock<DataLakeDirectoryClient>();

        const string pathWithUnknownExtension = "my_file.xxx";
        var pathItem = DataLakeModelFactory.PathItem(pathWithUnknownExtension, false, DateTimeOffset.Now, ETag.All, 1, "owner", "group", "permissions");
        var page = Page<PathItem>.FromValues(new[] { pathItem }, null, Moq.Mock.Of<Response>());
        var asyncPageable = AsyncPageable<PathItem>.FromPages(new[] { page });

        dataLakeDirectoryClientMock
            .Setup(client => client.GetPathsAsync(false, false, It.IsAny<CancellationToken>()))
            .Returns(asyncPageable);

        response.Setup(res => res.Value).Returns(true);

        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);

        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(response.Object);

        var sut = new BatchFileManager(dataLakeFileSystemClientMock.Object, webFileZipperMock.Object);

        // Act and Assert
        await sut
            .Invoking(s => s.GetResultFileStreamAsync(Guid.NewGuid(), new GridAreaCode("123")))
            .Should()
            .ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetResultFileStreamAsync_ReturnsStream()
    {
        // Arrange
        var webFileZipperMock = new Mock<IWebFilesZipper>();
        var response = new Mock<Response<bool>>();
        var dataLakeFileSystemClientMock = new Mock<DataLakeFileSystemClient>();
        var dataLakeDirectoryClientMock = new Mock<DataLakeDirectoryClient>();
        var dataLakeFileClientMock = new Mock<DataLakeFileClient>();

        const string pathWithKnownExtension = "my_file.json";
        var pathItem = DataLakeModelFactory
            .PathItem(pathWithKnownExtension, false, DateTimeOffset.Now, ETag.All, 1, "owner", "group", "permissions");
        var page = Page<PathItem>.FromValues(new[] { pathItem }, null, Moq.Mock.Of<Response>());
        var asyncPageable = AsyncPageable<PathItem>.FromPages(new[] { page });

        dataLakeDirectoryClientMock
            .Setup(client => client.GetPathsAsync(false, false, It.IsAny<CancellationToken>()))
            .Returns(asyncPageable);

        response.Setup(res => res.Value).Returns(true);

        dataLakeDirectoryClientMock.Setup(dirClient => dirClient.ExistsAsync(default))
            .ReturnsAsync(response.Object);

        dataLakeFileSystemClientMock.Setup(x => x.GetDirectoryClient(It.IsAny<string>()))
            .Returns(dataLakeDirectoryClientMock.Object);

        dataLakeFileSystemClientMock.Setup(x => x.GetFileClient(pathWithKnownExtension)).Returns(dataLakeFileClientMock.Object);

        var stream = new Mock<Stream>();
        dataLakeFileClientMock.Setup(x => x.OpenReadAsync(It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<int?>(), default)).ReturnsAsync(stream.Object);

        var sut = new BatchFileManager(dataLakeFileSystemClientMock.Object, webFileZipperMock.Object);

        // Act
        var actual = await sut.GetResultFileStreamAsync(Guid.NewGuid(), new GridAreaCode("123"));

        // Assert
        actual.Should().BeSameAs(stream.Object);
    }
}
