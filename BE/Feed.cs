using Nest;
using System;

namespace BE
{
    [ElasticsearchType(Name = "feed", IdProperty = "Id")]
    public class Feed
    {
        public Feed(string name, string link, int id = 0, bool favorite = false)
        {
            Id = id;
            Name = name;
            Link = link;
            isFavarite = favorite;
        }

        #region [[Properties]]
        [Number(Ignore = true)]
        public int Id { get; set; }

        /// name field
        /// </summary>
        [String(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// link field
        /// </summary>
        [String(Name = "link")]
        public string Link { get; set; }

        [Boolean(Name = "isFavorite")]
        public bool isFavarite { get; set; }

        #endregion
    }
}
