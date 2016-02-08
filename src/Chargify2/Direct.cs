﻿using System;
using System.Collections;
using System.Text;

namespace Chargify2
{
    public class Direct
    {
        readonly Client _client;

        public Direct(Client client)
        {
            _client = client;

            ValidateClient();
        }

        public static string Signature(string message, string secret)
        {
            return message.CalculateSignature(secret);
        }


        public SecureParameters SecureParameters(Hashtable hash)
        {
            return new SecureParameters(hash, _client);
        }

        public ResponseParameters ResponseParameters(Hashtable hash)
        {
            return new ResponseParameters(hash, _client);
        }

        private void ValidateClient()
        {
            if (_client == null)
                throw new ArgumentNullException(message: "Direct requires a Client as an argument", paramName: "client");
        }
    }

    public class SecureParameters
    {
        public string api_id { get; set; }
        public string timestamp { get; set; }
        public string nonce { get; set; }
        public Hashtable data { get; set; }
        public string secret { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hash">The hash to use when constructing the secure parameter hidden inputs</param>
        /// <param name="client">The client</param>
        public SecureParameters(Hashtable hash, Client client)
        {
            api_id = client.ApiKey;
            secret = client.ApiSecret;

            timestamp = (string)hash["timestamp"];
            nonce = (string)hash["nonce"];

            data = hash;

            ValidateArgs();
        }

        /// <summary>
        /// Converts the hash to a set of hidden inputs
        /// </summary>
        /// <returns></returns>
        public string ToFormInputs()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(string.Format(format: "<input type='hidden' name='secure[api_id]' value='{0}'/>", arg0: api_id));
            sb.AppendLine(string.Format(format: "<input type='hidden' name='secure[timestamp]' value='{0}'/>", arg0: timestamp));
            sb.AppendLine(string.Format(format: "<input type='hidden' name='secure[nonce]' value='{0}'/>", arg0: nonce));
            sb.AppendLine(string.Format(format: "<input type='hidden' name='secure[data]' value='{0}'/>", arg0: EncodedData));
            sb.AppendLine(string.Format(format: "<input type='hidden' name='secure[signature]' value='{0}'/>", arg0: Signature));
            return sb.ToString();
        }

        /// <summary>
        /// The information that should be included in the secure[data] field
        /// </summary>
        public string EncodedData
        {
            get
            {
                Hashtable h = data;
                if (h.ContainsKey(key: "timestamp")) h.Remove(key: "timestamp");
                if (h.ContainsKey(key: "nonce")) h.Remove(key: "nonce");
                return h.ToQueryString();
            }
        }

        /// <summary>
        /// The signature, as calculated by the formula listed in the documentation
        /// </summary>
        private string Signature
        {
            get
            {
                var message = api_id + timestamp + nonce + EncodedData;
                return Direct.Signature(message, secret);
            }
        }

        public void ValidateArgs()
        {
            if (data == null)
            {
                throw new ArgumentException(message: "The 'data' provided must be provideed as a Hash", paramName: "data");
            }

            if (string.IsNullOrWhiteSpace(api_id) || string.IsNullOrWhiteSpace(secret))
            {
                throw new ArgumentException(message: "SecureParameters requires connection to a Client - was one given?");
            }
        }
    }

    /// <summary>
    /// Based on the work in the Chargify2 gem (https://github.com/chargify/chargify2)
    /// </summary>
    public class ResponseParameters
    {
        public string api_id { get; set; }
        public string timestamp { get; set; }
        public string nonce { get; set; }
        public string status_code { get; set; }
        public string result_code { get; set; }
        public string call_id { get; set; }
        public string secret { get; private set; }
        public string signature { get; set; }

        public ResponseParameters(Hashtable hash, Client client)
        {
            api_id = client.ApiKey;
            secret = client.ApiSecret;

            status_code = (string)hash["status_code"];
            timestamp = (string)hash["timestamp"];
            nonce = (string)hash["nonce"];
            result_code = (string)hash["result_code"];
            call_id = (string)hash["call_id"];
            signature = (string)hash["signature"];

            ValidateArgs();
        }

        /// <summary>
        /// Was this successful?
        /// </summary>
        public bool isSuccess
        {
            get 
            {
                return status_code == "200";
            }
        }

        /// <summary>
        /// Is the signature returned from the call valid?
        /// </summary>
        public bool isVerified
        {
            get
            {
                var message = api_id + timestamp + nonce + status_code + result_code + call_id;
                return Direct.Signature(message, secret) == signature;
            }
        }

        public void ValidateArgs()
        {
            if (string.IsNullOrWhiteSpace(api_id) || string.IsNullOrWhiteSpace(secret))
            {
                throw new ArgumentException("ResponseParameters requires connection to a Client - was one given?");
            }
        }
    }
}
