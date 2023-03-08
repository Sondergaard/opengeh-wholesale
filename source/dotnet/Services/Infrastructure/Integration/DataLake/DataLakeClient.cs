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

using System.IdentityModel.Tokens.Jwt;
using Azure.Storage.Files.DataLake;

namespace Energinet.DataHub.Wholesale.Infrastructure.Integration.DataLake;

public sealed class DataLakeClient : IDataLakeClient
{
    private readonly DataLakeFileSystemClient _dataLakeFileSystemClient;

    public DataLakeClient(DataLakeFileSystemClient dataLakeFileSystemClient)
    {
        _dataLakeFileSystemClient = dataLakeFileSystemClient;
    }

    /// <summary>
    /// Search for a file by a given extension in a blob directory.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="extension"></param>
    /// <returns>The first file with matching file extension. If the file cannot be found, an exception is thrown</returns>
    public async Task<Stream> FindAndOpenFileAsync(string directory, string extension)
    {
        var path = await FindFileAsync(directory, extension).ConfigureAwait(false);
        var dataLakeFileClient = _dataLakeFileSystemClient.GetFileClient(path);
        return await dataLakeFileClient.OpenReadAsync(false).ConfigureAwait(false);
    }

    private async Task<string> FindFileAsync(string directory, string extension)
    {
        var directoryClient = _dataLakeFileSystemClient.GetDirectoryClient(directory);
        var directoryExists = await directoryClient.ExistsAsync().ConfigureAwait(false);
        if (!directoryExists.Value)
            throw new InvalidOperationException($"No directory was found on path: {directory}");

        await foreach (var pathItem in directoryClient.GetPathsAsync())
        {
            if (Path.GetExtension(pathItem.Name) == extension)
                return pathItem.Name;
        }

        throw new Exception($"No Data Lake file with extension '{extension}' was found in directory '{directory}'");
    }

    private async Task<Stream> OpenReadAsync(string path)
    {
        var dataLakeFileClient = _dataLakeFileSystemClient.GetFileClient(path);
        return await dataLakeFileClient.OpenReadAsync(false).ConfigureAwait(false);
    }
}
