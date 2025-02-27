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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Batches.Application;
using Energinet.DataHub.Wholesale.Batches.Application.UseCases;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Batches.UnitTests.Application.Batches;

[UnitTest]
public class UpdateExecutionStateHandlerTests
{
    [Theory]
    [InlineAutoMoqData]
    public async Task UpdateExecutionStateAsync_ActivatesDomainServiceAndCommits(
        [Frozen] Mock<IUnitOfWork> unitOfWorkMock,
        [Frozen] Mock<IBatchExecutionStateInfrastructureService> calculationDomainServiceMock,
        UpdateBatchExecutionStateHandler sut)
    {
        // Arrange & Act
        await sut.UpdateAsync();

        // Assert
        unitOfWorkMock.Verify(x => x.CommitAsync());
        calculationDomainServiceMock.Verify(x => x.UpdateExecutionStateAsync());
    }
}
