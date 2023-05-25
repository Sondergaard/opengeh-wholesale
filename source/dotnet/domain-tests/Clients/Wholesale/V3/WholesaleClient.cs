// Copyright 2020 Energinet DataHub A/S
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

using System.IO;

#pragma warning disable 108 // Disable "CS0108 '{derivedDto}.ToJson()' hides inherited member '{dtoBase}.ToJson()'. Use the new keyword if hiding was intended."
#pragma warning disable 114 // Disable "CS0114 '{derivedDto}.RaisePropertyChanged(String)' hides inherited member 'dtoBase.RaisePropertyChanged(String)'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword."
#pragma warning disable 472 // Disable "CS0472 The result of the expression is always 'false' since a value of type 'Int32' is never equal to 'null' of type 'Int32?'
#pragma warning disable 1573 // Disable "CS1573 Parameter '...' has no matching param tag in the XML comment for ...
#pragma warning disable 1591 // Disable "CS1591 Missing XML comment for publicly visible type or member ..."
#pragma warning disable 8073 // Disable "CS8073 The result of the expression is always 'false' since a value of type 'T' is never equal to 'null' of type 'T?'"
#pragma warning disable 3016 // Disable "CS3016 Arrays as attribute arguments is not CLS-compliant"
#pragma warning disable 8603 // Disable "CS8603 Possible null reference return"

namespace Energinet.DataHub.Wholesale.DomainTests.Clients.v3
{
    using System = global::System;

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial interface IWholesaleClient_V3
    {

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Create a batch.
        /// </summary>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<System.Guid> CreateBatchAsync(BatchRequestDto body, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Get batches that matches the criteria specified
        /// </summary>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<System.Collections.Generic.ICollection<BatchDto>> SearchBatchesAsync(System.Collections.Generic.IEnumerable<string> gridAreaCodes = null, BatchState? executionState = null, System.DateTimeOffset? minExecutionTime = null, System.DateTimeOffset? maxExecutionTime = null, System.DateTimeOffset? periodStart = null, System.DateTimeOffset? periodEnd = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Returns a batch matching batchId.
        /// </summary>
        /// <param name="batchId">BatchId</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BatchDto> GetBatchAsync(System.Guid batchId, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Balance responsible parties.
        /// </summary>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<System.Collections.Generic.ICollection<ActorDto>> GetListOfBalanceResponsiblePartiesAsync(System.Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string api_version = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Returns a list of Energy suppliers. If balance responsible party is specified by the balanceResponsibleParty parameter only the energy suppliers associated with that balance responsible party is returned
        /// </summary>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<System.Collections.Generic.ICollection<ActorDto>> GetListOfEnergySuppliersAsync(System.Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string balanceResponsibleParty = null, string api_version = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Calculation results provided by the following method:
        /// <br/>When only 'energySupplierGln' is provided, a result is returned for a energy supplier for the requested grid area, for the specified time series type.
        /// <br/>if only a 'balanceResponsiblePartyGln' is provided, a result is returned for a balance responsible party for the requested grid area, for the specified time series type.
        /// <br/>if both 'balanceResponsiblePartyGln' and 'energySupplierGln' is provided, a result is returned for the balance responsible party's energy supplier for requested grid area, for the specified time series type.
        /// <br/>if no 'balanceResponsiblePartyGln' and 'energySupplierGln' is provided, a result is returned for the requested grid area, for the specified time series type.
        /// </summary>
        /// <param name="batchId">The id to identify the batch the request is for</param>
        /// <param name="gridAreaCode">The grid area the requested result is in</param>
        /// <param name="timeSeriesType">The time series type the result has</param>
        /// <param name="energySupplierGln">The GLN for the energy supplier the requested result</param>
        /// <param name="balanceResponsiblePartyGln">The GLN for the balance responsible party the requested result</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<ProcessStepResultDto> GetProcessStepResultAsync(System.Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string energySupplierGln = null, string balanceResponsiblePartyGln = null, string api_version = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Downloads a compressed settlement report for the specified parameters.
        /// </summary>
        /// <param name="gridAreaCodes">A list of grid areas to create the settlement report for.</param>
        /// <param name="processType">Currently expects BalanceFixing only.</param>
        /// <param name="periodStart">The start date and time of the period covered by the settlement report.</param>
        /// <param name="periodEnd">The end date and time of the period covered by the settlement report.</param>
        /// <param name="energySupplier">Optional GLN/EIC identifier for an energy supplier.</param>
        /// <param name="csvFormatLocale">Optional locale used to format the CSV file, e.g. da-DK. Defaults to en-US.</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<FileResponse> DownloadAsync(System.Collections.Generic.IEnumerable<string> gridAreaCodes, ProcessType processType, System.DateTimeOffset periodStart, System.DateTimeOffset periodEnd, string energySupplier = null, string csvFormatLocale = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Returns a stream containing the settlement report for batch with batchId and gridAreaCode.
        /// </summary>
        /// <param name="batchId">BatchId</param>
        /// <param name="gridAreaCode">GridAreaCode</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<FileResponse> GetSettlementReportAsStreamAsync(System.Guid batchId, string gridAreaCode, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Returns a stream containing the settlement report for a batch matching batchId
        /// </summary>
        /// <param name="batchId">BatchId</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<FileResponse> ZippedBasisDataStreamAsync(System.Guid batchId, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class WholesaleClient_V3 : IWholesaleClient_V3
    {
        private string _baseUrl = "";
        private System.Net.Http.HttpClient _httpClient;
        private System.Lazy<Newtonsoft.Json.JsonSerializerSettings> _settings;

        public WholesaleClient_V3(string baseUrl, System.Net.Http.HttpClient httpClient)
        {
            BaseUrl = baseUrl;
            _httpClient = httpClient;
            _settings = new System.Lazy<Newtonsoft.Json.JsonSerializerSettings>(CreateSerializerSettings);
        }

        private Newtonsoft.Json.JsonSerializerSettings CreateSerializerSettings()
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            UpdateJsonSerializerSettings(settings);
            return settings;
        }

        public string BaseUrl
        {
            get { return _baseUrl; }
            set { _baseUrl = value; }
        }

        public Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings { get { return _settings.Value; } }

        partial void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings);

        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url);
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder);
        partial void ProcessResponse(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Create a batch.
        /// </summary>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<System.Guid> CreateBatchAsync(BatchRequestDto body, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (body == null)
                throw new System.ArgumentNullException("body");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/v3/batches");

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    var json_ = Newtonsoft.Json.JsonConvert.SerializeObject(body, _settings.Value);
                    var content_ = new System.Net.Http.StringContent(json_);
                    content_.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                    request_.Content = content_;
                    request_.Method = new System.Net.Http.HttpMethod("POST");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<System.Guid>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Get batches that matches the criteria specified
        /// </summary>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<System.Collections.Generic.ICollection<BatchDto>> SearchBatchesAsync(System.Collections.Generic.IEnumerable<string> gridAreaCodes = null, BatchState? executionState = null, System.DateTimeOffset? minExecutionTime = null, System.DateTimeOffset? maxExecutionTime = null, System.DateTimeOffset? periodStart = null, System.DateTimeOffset? periodEnd = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/v3/batches?");
            if (gridAreaCodes != null)
            {
                foreach (var item_ in gridAreaCodes) { urlBuilder_.Append(System.Uri.EscapeDataString("gridAreaCodes") + "=").Append(System.Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&"); }
            }
            if (executionState != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("executionState") + "=").Append(System.Uri.EscapeDataString(ConvertToString(executionState, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (minExecutionTime != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("minExecutionTime") + "=").Append(System.Uri.EscapeDataString(minExecutionTime.Value.ToString("s", System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (maxExecutionTime != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("maxExecutionTime") + "=").Append(System.Uri.EscapeDataString(maxExecutionTime.Value.ToString("s", System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (periodStart != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("periodStart") + "=").Append(System.Uri.EscapeDataString(periodStart.Value.ToString("s", System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (periodEnd != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("periodEnd") + "=").Append(System.Uri.EscapeDataString(periodEnd.Value.ToString("s", System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<System.Collections.Generic.ICollection<BatchDto>>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Returns a batch matching batchId.
        /// </summary>
        /// <param name="batchId">BatchId</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<BatchDto> GetBatchAsync(System.Guid batchId, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (batchId == null)
                throw new System.ArgumentNullException("batchId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/v3/batches/{batchId}");
            urlBuilder_.Replace("{batchId}", System.Uri.EscapeDataString(ConvertToString(batchId, System.Globalization.CultureInfo.InvariantCulture)));

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<BatchDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Balance responsible parties.
        /// </summary>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<System.Collections.Generic.ICollection<ActorDto>> GetListOfBalanceResponsiblePartiesAsync(System.Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string api_version = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (batchId == null)
                throw new System.ArgumentNullException("batchId");

            if (gridAreaCode == null)
                throw new System.ArgumentNullException("gridAreaCode");

            if (timeSeriesType == null)
                throw new System.ArgumentNullException("timeSeriesType");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/v3/batches/{batchId}/processes/{gridAreaCode}/time-series-types/{timeSeriesType}/balance-responsible-parties?");
            urlBuilder_.Replace("{batchId}", System.Uri.EscapeDataString(ConvertToString(batchId, System.Globalization.CultureInfo.InvariantCulture)));
            urlBuilder_.Replace("{gridAreaCode}", System.Uri.EscapeDataString(ConvertToString(gridAreaCode, System.Globalization.CultureInfo.InvariantCulture)));
            urlBuilder_.Replace("{timeSeriesType}", System.Uri.EscapeDataString(ConvertToString(timeSeriesType, System.Globalization.CultureInfo.InvariantCulture)));
            if (api_version != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("api-version") + "=").Append(System.Uri.EscapeDataString(ConvertToString(api_version, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<System.Collections.Generic.ICollection<ActorDto>>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Returns a list of Energy suppliers. If balance responsible party is specified by the balanceResponsibleParty parameter only the energy suppliers associated with that balance responsible party is returned
        /// </summary>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<System.Collections.Generic.ICollection<ActorDto>> GetListOfEnergySuppliersAsync(System.Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string balanceResponsibleParty = null, string api_version = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (batchId == null)
                throw new System.ArgumentNullException("batchId");

            if (gridAreaCode == null)
                throw new System.ArgumentNullException("gridAreaCode");

            if (timeSeriesType == null)
                throw new System.ArgumentNullException("timeSeriesType");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/v3/batches/{batchId}/processes/{gridAreaCode}/time-series-types/{timeSeriesType}/energy-suppliers?");
            urlBuilder_.Replace("{batchId}", System.Uri.EscapeDataString(ConvertToString(batchId, System.Globalization.CultureInfo.InvariantCulture)));
            urlBuilder_.Replace("{gridAreaCode}", System.Uri.EscapeDataString(ConvertToString(gridAreaCode, System.Globalization.CultureInfo.InvariantCulture)));
            urlBuilder_.Replace("{timeSeriesType}", System.Uri.EscapeDataString(ConvertToString(timeSeriesType, System.Globalization.CultureInfo.InvariantCulture)));
            if (balanceResponsibleParty != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("balanceResponsibleParty") + "=").Append(System.Uri.EscapeDataString(ConvertToString(balanceResponsibleParty, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (api_version != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("api-version") + "=").Append(System.Uri.EscapeDataString(ConvertToString(api_version, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<System.Collections.Generic.ICollection<ActorDto>>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Calculation results provided by the following method:
        /// <br/>When only 'energySupplierGln' is provided, a result is returned for a energy supplier for the requested grid area, for the specified time series type.
        /// <br/>if only a 'balanceResponsiblePartyGln' is provided, a result is returned for a balance responsible party for the requested grid area, for the specified time series type.
        /// <br/>if both 'balanceResponsiblePartyGln' and 'energySupplierGln' is provided, a result is returned for the balance responsible party's energy supplier for requested grid area, for the specified time series type.
        /// <br/>if no 'balanceResponsiblePartyGln' and 'energySupplierGln' is provided, a result is returned for the requested grid area, for the specified time series type.
        /// </summary>
        /// <param name="batchId">The id to identify the batch the request is for</param>
        /// <param name="gridAreaCode">The grid area the requested result is in</param>
        /// <param name="timeSeriesType">The time series type the result has</param>
        /// <param name="energySupplierGln">The GLN for the energy supplier the requested result</param>
        /// <param name="balanceResponsiblePartyGln">The GLN for the balance responsible party the requested result</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<ProcessStepResultDto> GetProcessStepResultAsync(System.Guid batchId, string gridAreaCode, TimeSeriesType timeSeriesType, string energySupplierGln = null, string balanceResponsiblePartyGln = null, string api_version = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (batchId == null)
                throw new System.ArgumentNullException("batchId");

            if (gridAreaCode == null)
                throw new System.ArgumentNullException("gridAreaCode");

            if (timeSeriesType == null)
                throw new System.ArgumentNullException("timeSeriesType");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/v3/batches/{batchId}/processes/{gridAreaCode}/time-series-types/{timeSeriesType}?");
            urlBuilder_.Replace("{batchId}", System.Uri.EscapeDataString(ConvertToString(batchId, System.Globalization.CultureInfo.InvariantCulture)));
            urlBuilder_.Replace("{gridAreaCode}", System.Uri.EscapeDataString(ConvertToString(gridAreaCode, System.Globalization.CultureInfo.InvariantCulture)));
            urlBuilder_.Replace("{timeSeriesType}", System.Uri.EscapeDataString(ConvertToString(timeSeriesType, System.Globalization.CultureInfo.InvariantCulture)));
            if (energySupplierGln != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("energySupplierGln") + "=").Append(System.Uri.EscapeDataString(ConvertToString(energySupplierGln, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (balanceResponsiblePartyGln != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("balanceResponsiblePartyGln") + "=").Append(System.Uri.EscapeDataString(ConvertToString(balanceResponsiblePartyGln, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (api_version != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("api-version") + "=").Append(System.Uri.EscapeDataString(ConvertToString(api_version, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200)
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ProcessStepResultDto>(response_, headers_, cancellationToken).ConfigureAwait(false);
                            if (objectResponse_.Object == null)
                            {
                                throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                            }
                            return objectResponse_.Object;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Downloads a compressed settlement report for the specified parameters.
        /// </summary>
        /// <param name="gridAreaCodes">A list of grid areas to create the settlement report for.</param>
        /// <param name="processType">Currently expects BalanceFixing only.</param>
        /// <param name="periodStart">The start date and time of the period covered by the settlement report.</param>
        /// <param name="periodEnd">The end date and time of the period covered by the settlement report.</param>
        /// <param name="energySupplier">Optional GLN/EIC identifier for an energy supplier.</param>
        /// <param name="csvFormatLocale">Optional locale used to format the CSV file, e.g. da-DK. Defaults to en-US.</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<FileResponse> DownloadAsync(System.Collections.Generic.IEnumerable<string> gridAreaCodes, ProcessType processType, System.DateTimeOffset periodStart, System.DateTimeOffset periodEnd, string energySupplier = null, string csvFormatLocale = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (gridAreaCodes == null)
                throw new System.ArgumentNullException("gridAreaCodes");

            if (processType == null)
                throw new System.ArgumentNullException("processType");

            if (periodStart == null)
                throw new System.ArgumentNullException("periodStart");

            if (periodEnd == null)
                throw new System.ArgumentNullException("periodEnd");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/v3/SettlementReport/Download?");
            foreach (var item_ in gridAreaCodes) { urlBuilder_.Append(System.Uri.EscapeDataString("gridAreaCodes") + "=").Append(System.Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append("&"); }
            urlBuilder_.Append(System.Uri.EscapeDataString("processType") + "=").Append(System.Uri.EscapeDataString(ConvertToString(processType, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            urlBuilder_.Append(System.Uri.EscapeDataString("periodStart") + "=").Append(System.Uri.EscapeDataString(periodStart.ToString("s", System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            urlBuilder_.Append(System.Uri.EscapeDataString("periodEnd") + "=").Append(System.Uri.EscapeDataString(periodEnd.ToString("s", System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            if (energySupplier != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("energySupplier") + "=").Append(System.Uri.EscapeDataString(ConvertToString(energySupplier, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            if (csvFormatLocale != null)
            {
                urlBuilder_.Append(System.Uri.EscapeDataString("csvFormatLocale") + "=").Append(System.Uri.EscapeDataString(ConvertToString(csvFormatLocale, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            }
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/octet-stream"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200 || status_ == 206)
                        {
                            var responseStream_ = response_.Content == null ? System.IO.Stream.Null : await response_.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            var fileResponse_ = new FileResponse(status_, headers_, responseStream_, null, response_);
                            disposeClient_ = false; disposeResponse_ = false; // response and client are disposed by FileResponse
                            return fileResponse_;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Returns a stream containing the settlement report for batch with batchId and gridAreaCode.
        /// </summary>
        /// <param name="batchId">BatchId</param>
        /// <param name="gridAreaCode">GridAreaCode</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<FileResponse> GetSettlementReportAsStreamAsync(System.Guid batchId, string gridAreaCode, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (batchId == null)
                throw new System.ArgumentNullException("batchId");

            if (gridAreaCode == null)
                throw new System.ArgumentNullException("gridAreaCode");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/v3/SettlementReport?");
            urlBuilder_.Append(System.Uri.EscapeDataString("batchId") + "=").Append(System.Uri.EscapeDataString(ConvertToString(batchId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            urlBuilder_.Append(System.Uri.EscapeDataString("gridAreaCode") + "=").Append(System.Uri.EscapeDataString(ConvertToString(gridAreaCode, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/octet-stream"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200 || status_ == 206)
                        {
                            var responseStream_ = response_.Content == null ? System.IO.Stream.Null : await response_.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            var fileResponse_ = new FileResponse(status_, headers_, responseStream_, null, response_);
                            disposeClient_ = false; disposeResponse_ = false; // response and client are disposed by FileResponse
                            return fileResponse_;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// Returns a stream containing the settlement report for a batch matching batchId
        /// </summary>
        /// <param name="batchId">BatchId</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public virtual async System.Threading.Tasks.Task<FileResponse> ZippedBasisDataStreamAsync(System.Guid batchId, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            if (batchId == null)
                throw new System.ArgumentNullException("batchId");

            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/v3/SettlementReport/ZippedBasisDataStream?");
            urlBuilder_.Append(System.Uri.EscapeDataString("batchId") + "=").Append(System.Uri.EscapeDataString(ConvertToString(batchId, System.Globalization.CultureInfo.InvariantCulture))).Append("&");
            urlBuilder_.Length--;

            var client_ = _httpClient;
            var disposeClient_ = false;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    request_.Method = new System.Net.Http.HttpMethod("GET");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/octet-stream"));

                    PrepareRequest(client_, request_, urlBuilder_);

                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);

                    PrepareRequest(client_, request_, url_);

                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    var disposeResponse_ = true;
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }

                        ProcessResponse(client_, response_);

                        var status_ = (int)response_.StatusCode;
                        if (status_ == 200 || status_ == 206)
                        {
                            var responseStream_ = response_.Content == null ? System.IO.Stream.Null : await response_.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            var fileResponse_ = new FileResponse(status_, headers_, responseStream_, null, response_);
                            disposeClient_ = false; disposeResponse_ = false; // response and client are disposed by FileResponse
                            return fileResponse_;
                        }
                        else
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                        }
                    }
                    finally
                    {
                        if (disposeResponse_)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
                if (disposeClient_)
                    client_.Dispose();
            }
        }

        protected struct ObjectResponseResult<T>
        {
            public ObjectResponseResult(T responseObject, string responseText)
            {
                this.Object = responseObject;
                this.Text = responseText;
            }

            public T Object { get; }

            public string Text { get; }
        }

        public bool ReadResponseAsString { get; set; }

        protected virtual async System.Threading.Tasks.Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(System.Net.Http.HttpResponseMessage response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Threading.CancellationToken cancellationToken)
        {
            if (response == null || response.Content == null)
            {
                return new ObjectResponseResult<T>(default(T), string.Empty);
            }

            if (ReadResponseAsString)
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var typedBody = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseText, JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
                }
            }
            else
            {
                try
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var streamReader = new System.IO.StreamReader(responseStream))
                    using (var jsonTextReader = new Newtonsoft.Json.JsonTextReader(streamReader))
                    {
                        var serializer = Newtonsoft.Json.JsonSerializer.Create(JsonSerializerSettings);
                        var typedBody = serializer.Deserialize<T>(jsonTextReader);
                        return new ObjectResponseResult<T>(typedBody, string.Empty);
                    }
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
                }
            }
        }

        private string ConvertToString(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return "";
            }

            if (value is System.Enum)
            {
                var name = System.Enum.GetName(value.GetType(), value);
                if (name != null)
                {
                    var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                    if (field != null)
                    {
                        var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute))
                            as System.Runtime.Serialization.EnumMemberAttribute;
                        if (attribute != null)
                        {
                            return attribute.Value != null ? attribute.Value : name;
                        }
                    }

                    var converted = System.Convert.ToString(System.Convert.ChangeType(value, System.Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                    return converted == null ? string.Empty : converted;
                }
            }
            else if (value is bool)
            {
                return System.Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
            }
            else if (value is byte[])
            {
                return System.Convert.ToBase64String((byte[]) value);
            }
            else if (value.GetType().IsArray)
            {
                var array = System.Linq.Enumerable.OfType<object>((System.Array) value);
                return string.Join(",", System.Linq.Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
            }

            var result = System.Convert.ToString(value, cultureInfo);
            return result == null ? "" : result;
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class ActorDto
    {
        [Newtonsoft.Json.JsonProperty("gln", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Gln { get; set; }

    }

    /// <summary>
    /// An immutable batch.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class BatchDto
    {
        [Newtonsoft.Json.JsonProperty("runId", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long? RunId { get; set; }

        [Newtonsoft.Json.JsonProperty("batchId", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Guid BatchId { get; set; }

        [Newtonsoft.Json.JsonProperty("periodStart", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset PeriodStart { get; set; }

        [Newtonsoft.Json.JsonProperty("periodEnd", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset PeriodEnd { get; set; }

        [Newtonsoft.Json.JsonProperty("resolution", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Resolution { get; set; }

        [Newtonsoft.Json.JsonProperty("unit", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Unit { get; set; }

        [Newtonsoft.Json.JsonProperty("executionTimeStart", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset? ExecutionTimeStart { get; set; }

        [Newtonsoft.Json.JsonProperty("executionTimeEnd", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset? ExecutionTimeEnd { get; set; }

        [Newtonsoft.Json.JsonProperty("executionState", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public BatchState ExecutionState { get; set; }

        [Newtonsoft.Json.JsonProperty("areSettlementReportsCreated", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool AreSettlementReportsCreated { get; set; }

        [Newtonsoft.Json.JsonProperty("gridAreaCodes", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<string> GridAreaCodes { get; set; }

        [Newtonsoft.Json.JsonProperty("processType", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public ProcessType ProcessType { get; set; }

        [Newtonsoft.Json.JsonProperty("createdByUserId", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Guid CreatedByUserId { get; set; }

    }

    /// <summary>
    /// An immutable request to create a batch.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class BatchRequestDto
    {
        [Newtonsoft.Json.JsonProperty("processType", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public ProcessType ProcessType { get; set; }

        [Newtonsoft.Json.JsonProperty("gridAreaCodes", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<string> GridAreaCodes { get; set; }

        [Newtonsoft.Json.JsonProperty("startDate", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset StartDate { get; set; }

        [Newtonsoft.Json.JsonProperty("endDate", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset EndDate { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public enum BatchState
    {

        [System.Runtime.Serialization.EnumMember(Value = @"Pending")]
        Pending = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"Executing")]
        Executing = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"Completed")]
        Completed = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"Failed")]
        Failed = 3,

    }

    /// <summary>
    /// Result data from a specific step in a process
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class ProcessStepResultDto
    {
        /// <summary>
        /// Sum has a scale of 3
        /// </summary>
        [Newtonsoft.Json.JsonProperty("sum", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double Sum { get; set; }

        /// <summary>
        /// Min has a scale of 3
        /// </summary>
        [Newtonsoft.Json.JsonProperty("min", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double Min { get; set; }

        /// <summary>
        /// Max has a scale of 3
        /// </summary>
        [Newtonsoft.Json.JsonProperty("max", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double Max { get; set; }

        [Newtonsoft.Json.JsonProperty("periodStart", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset PeriodStart { get; set; }

        [Newtonsoft.Json.JsonProperty("periodEnd", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset PeriodEnd { get; set; }

        [Newtonsoft.Json.JsonProperty("resolution", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Resolution { get; set; }

        /// <summary>
        /// kWh
        /// </summary>
        [Newtonsoft.Json.JsonProperty("unit", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Unit { get; set; }

        [Newtonsoft.Json.JsonProperty("timeSeriesPoints", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<TimeSeriesPointDto> TimeSeriesPoints { get; set; }

        [Newtonsoft.Json.JsonProperty("processType", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public ProcessType ProcessType { get; set; }

        [Newtonsoft.Json.JsonProperty("timeSeriesType", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public TimeSeriesType TimeSeriesType { get; set; }

    }

    /// <summary>
    /// Defines the wholesale process type
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public enum ProcessType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"BalanceFixing")]
        BalanceFixing = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"Aggregation")]
        Aggregation = 1,

    }

    /// <summary>
    /// TimeSeriesPoint
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class TimeSeriesPointDto
    {
        /// <summary>
        /// The observation time for the measured 'Quantity'
        /// </summary>
        [Newtonsoft.Json.JsonProperty("time", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset Time { get; set; }

        /// <summary>
        /// Quantity has a scale of 3
        /// </summary>
        [Newtonsoft.Json.JsonProperty("quantity", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double Quantity { get; set; }

        /// <summary>
        /// Any of the values from Energinet.DataHub.Wholesale.WebApi.V3.ProcessStepResult.TimeSeriesPointQuality
        /// </summary>
        [Newtonsoft.Json.JsonProperty("quality", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Quality { get; set; }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public enum TimeSeriesType
    {

        [System.Runtime.Serialization.EnumMember(Value = @"NonProfiledConsumption")]
        NonProfiledConsumption = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"FlexConsumption")]
        FlexConsumption = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"Production")]
        Production = 2,

        [System.Runtime.Serialization.EnumMember(Value = @"NetExchangePerGridArea")]
        NetExchangePerGridArea = 3,

    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class FileResponse : System.IDisposable
    {
        private System.IDisposable _client;
        private System.IDisposable _response;

        public int StatusCode { get; private set; }

        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> Headers { get; private set; }

        public System.IO.Stream Stream { get; private set; }

        public bool IsPartial
        {
            get { return StatusCode == 206; }
        }

        public FileResponse(int statusCode, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.IO.Stream stream, System.IDisposable client, System.IDisposable response)
        {
            StatusCode = statusCode;
            Headers = headers;
            Stream = stream;
            _client = client;
            _response = response;
        }

        public void Dispose()
        {
            Stream.Dispose();
            if (_response != null)
                _response.Dispose();
            if (_client != null)
                _client.Dispose();
        }
    }


    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class ApiException : System.Exception
    {
        public int StatusCode { get; private set; }

        public string Response { get; private set; }

        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> Headers { get; private set; }

        public ApiException(string message, int statusCode, string response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Exception innerException)
            : base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + ((response == null) ? "(null)" : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
        {
            StatusCode = statusCode;
            Response = response;
            Headers = headers;
        }

        public override string ToString()
        {
            return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class ApiException<TResult> : ApiException
    {
        public TResult Result { get; private set; }

        public ApiException(string message, int statusCode, string response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, TResult result, System.Exception innerException)
            : base(message, statusCode, response, headers, innerException)
        {
            Result = result;
        }
    }

}

#pragma warning restore 1591
#pragma warning restore 1573
#pragma warning restore  472
#pragma warning restore  114
#pragma warning restore  108
#pragma warning restore 3016
#pragma warning restore 8603
