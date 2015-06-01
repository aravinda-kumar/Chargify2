﻿using System;
using Chargify2.Configuration;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Deserializers;

namespace Chargify2
{
    public partial class Client : IClient
    {
        const string BaseUrl = "https://api.chargify.com/api/v2";

        readonly string _apiKey;
        readonly string _apiPassword;
        readonly string _apiSecret;
        readonly Uri _proxy;

        public Client(string apiKey, string apiPassword, string apiSecret, Uri proxy)
        {
            this._apiKey = apiKey;
            this._apiPassword = apiPassword;
            this._apiSecret = apiSecret;
            this._proxy = proxy;
        }

        public Client(string apiKey, string apiPassword, string apiSecret)
            :this(apiKey, apiPassword, apiSecret, null)
        {
        }

        public Client(ChargifyAccountElement config)
            : this(config.ApiKey, config.ApiPassword, config.Secret, config.Proxy)
        {
        }

        private string _userAgent;
        private string UserAgent
        {
            get
            {
                if (_userAgent == null)
                {
                    _userAgent = String.Format("Chargify2 .NET Client v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                }
                return _userAgent;
            }
        }

        public string ApiKey
        {
            get
            {
                return this._apiKey;
            }
        }

        public string ApiPassword
        {
            get
            {
                return this._apiPassword;
            }
        }

        public string ApiSecret
        {
            get
            {
                return this._apiSecret;
            }
        }

        public Uri Proxy
        {
            get
            {
                return this._proxy;
            }
        }


        public Direct Direct
        {
            get
            {
                return new Direct(this);
            }
        }
    }

    public class DynamicJsonDeserializer : IDeserializer
    {
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }

        public T Deserialize<T>(IRestResponse response)
        {
            return JsonConvert.DeserializeObject<dynamic>(response.Content);
        }
    }
}
