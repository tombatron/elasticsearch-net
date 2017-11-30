﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elasticsearch.Net
{
	/// <summary>
	/// Low level client that exposes all of elasticsearch API endpoints but leaves you in charge of building request and handling the response
	/// </summary>
	public partial class ElasticLowLevelClient : IElasticLowLevelClient
	{
		private readonly UrlFormatProvider _formatter;

		public IConnectionConfigurationValues Settings => this.Transport.Settings;
		public IElasticsearchSerializer Serializer => this.Transport.Settings.RequestResponseSerializer;

		protected ITransport<IConnectionConfigurationValues> Transport { get; set; }

		/// <summary>Instantiate a new low level elasticsearch client to http://localhost:9200</summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public ElasticLowLevelClient() : this(new Transport<IConnectionConfigurationValues>(new ConnectionConfiguration())) { }

		/// <summary>Instantiate a new low level elasticsearch client using the specified settings</summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public ElasticLowLevelClient(IConnectionConfigurationValues settings) : this(new Transport<IConnectionConfigurationValues>(settings ?? new ConnectionConfiguration())) { }

		/// <summary>
		/// Instantiate a new low level elasticsearch client explicitly specifying a custom transport setup
		/// </summary>
		public ElasticLowLevelClient(ITransport<IConnectionConfigurationValues> transport)
		{
			transport.ThrowIfNull(nameof(transport));
			transport.Settings.ThrowIfNull(nameof(transport.Settings));
			transport.Settings.RequestResponseSerializer.ThrowIfNull(nameof(transport.Settings.RequestResponseSerializer));

			this.Transport = transport;
			this._formatter = new UrlFormatProvider(this.Transport.Settings);
		}

		private string Url(FormattableString formattable) => formattable.ToString(_formatter);

		private TRequestParams _params<TRequestParams>(Func<TRequestParams, TRequestParams> requestParameters, string contentType = null, string accept = null)
			where TRequestParams : class, IRequestParameters, new()
		{
			var requestParams = requestParameters?.Invoke(new TRequestParams());
			if (contentType.IsNullOrEmpty()) return requestParams;

			requestParams = requestParams ?? new TRequestParams();
			if (requestParams.RequestConfiguration == null) requestParams.RequestConfiguration = new RequestConfiguration();
			if (!contentType.IsNullOrEmpty() && requestParams.RequestConfiguration.ContentType.IsNullOrEmpty())
				requestParams.RequestConfiguration.ContentType = contentType;
			if (!accept.IsNullOrEmpty() && requestParams.RequestConfiguration.Accept.IsNullOrEmpty())
				requestParams.RequestConfiguration.Accept = accept;
			return requestParams;
		}

		public ElasticsearchResponse<T> DoRequest<T>(HttpMethod method, string path, PostData data = null, IRequestParameters requestParameters = null)
			where T : class =>
			this.Transport.Request<T>(method, path, data, requestParameters);

		public Task<ElasticsearchResponse<T>> DoRequestAsync<T>(HttpMethod method, string path, CancellationToken cancellationToken, PostData data = null, IRequestParameters requestParameters = null)
			where T : class =>
			this.Transport.RequestAsync<T>(method, path, cancellationToken, data, requestParameters);
	}
}
