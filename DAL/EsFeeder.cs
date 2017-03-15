using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Newtonsoft;
using Elasticsearch.Net;

namespace DAL
{
    public class EsFeeder
    {
        #region [[Properties]]

        /// <summary>
        /// URI 
        /// </summary>
        private const string ES_URI = "http://localhost:9200";

        /// <summary>
        /// Elastic settings
        /// </summary>
        private ConnectionSettings _settings;

        /// <summary>
        /// Current instantiated client
        /// </summary>
        public ElasticClient Current { get; set; }

        #endregion

        #region [[Constructors]]

        /// <summary>
        /// Constructor
        /// </summary>
        public EsFeeder()
        {
            var node = new Uri(ES_URI);

            _settings = new ConnectionSettings(node);
            _settings.DefaultIndex(DTO.Constants.DEFAULT_INDEX);
            _settings.MapDefaultTypeNames(m => m.Add(typeof(DTO.Feed), DTO.Constants.DEFAULT_INDEX_TYPE));
            Current = new ElasticClient(_settings);
            Current.Map<DTO.Feed>(m => m.AutoMap());

        }

        #endregion
    }
}
