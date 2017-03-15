using System;
using Nest;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;



namespace BL
{
    public class Feed
    {
        #region [[Properties]]

        /// <summary>
        /// Elastic client
        /// </summary>
        DAL.EsFeeder _EsClientDAL;


        #endregion

        #region [[Constructors]]

        /// <summary>
        /// Constructor
        /// </summary>
        public Feed()
        {
            _EsClientDAL = new EsFeeder();
        }

        #endregion

        #region [[Methods]]

        /// <summary>
        /// Inserting or Updating a doc
        /// </summary>
        /// <param name="feed"></param>
        public void Index(IEnumerable<BE.FeedViewModelBase> feeds)
        {
            int i = 1;
            foreach (var feed in feeds)
            {
                var response = _EsClientDAL.Current.Index(new BE.Feed(feed.Name, feed.LinkAsString, i++), f => f.Type(BE.Constants.DEFAULT_INDEX_TYPE));
                if (response.Created == false && response.ServerError != null)
                    throw new Exception(response.ServerError.Error.ToString());
            }
        }
        public int GetSumFeeds()
        {

            QueryContainer queryById = new TermQuery() { Field = "_id" };

            var hits = _EsClientDAL.Current
                                   .Search<BE.Feed>(s => s.Query(q => q.MatchAll() && queryById))
                                   .Hits;

            return (hits as IReadOnlyCollection<IHit<BE.Feed>>).Count();
        }

        /// <summary>
        /// Deleting a row
        /// </summary>
        /// <param name="feed"></param>
        public void Delete()
        {
            int sum = GetSumFeeds();
            for (int id = 1; id <= sum; id++)
            {
                bool ll = _EsClientDAL.Current
                 .Delete(new Nest.DeleteRequest(BE.Constants.DEFAULT_INDEX,
                                                BE.Constants.DEFAULT_INDEX_TYPE,
                                                id.ToString().Trim())).Found;
            }
        }

        /// <summary>
        /// Querying by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<BE.Feed> QueryById(string id)
        {
            QueryContainer queryById = new TermQuery() { Field = "_id", Value = id.Trim() };

            var hits = _EsClientDAL.Current
                                   .Search<BE.Feed>(s => s.Query(q => q.MatchAll() && queryById))
                                   .Hits;

            List<BE.Feed> typedList = hits.Select(hit => ConvertHitToFeed(hit)).ToList();

            return typedList;
        }

    

        /// <summary>
        /// Anonymous method to translate from a Hit to our feed DTO
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        private static BE.Feed ConvertHitToFeed(IHit<BE.Feed> hit)
        {
            Func<IHit<BE.Feed>, BE.Feed> func = (x) =>
            {
                hit.Source.Id = Convert.ToInt32(hit.Id);
                return hit.Source;
            };

            return func.Invoke(hit);

            #region Notes
            /*
             Its a necessary workaround to get the "_id" property that remains in a upper level.
             Take this json return as sample:             

            {
            "_index": "crud_sample",
            "_type": "feed_Info",
            "_id": "4",                   <- ID of the row
            "_score": 1,
            "_source": {                  <- All other properties are in the "_source" level: 
               "age": 32,
               "birthday": "19830101",
               "enrollmentFee": 100.1,
               "hasChildren": false,
               "name": "Juan",
               "opinion": "¿Qué tal estás?"
            }

             */
            #endregion
        }

        /// <summary>
        /// Create a query using all fields with 'AND' operator
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        private IQueryContainer CreateSimpleQueryUsingAnd(BE.Feed feed)
        {
            QueryContainer queryContainer = null;

            queryContainer &= new TermQuery() { Field = "_id", Value = feed.Id };

            queryContainer &= new TermQuery() { Field = "name", Value = feed.Name };

            queryContainer &= new TermQuery() { Field = "linq", Value = feed.Link };

            return queryContainer;
        }

        /// <summary>
        /// Create a query using all fields with 'AND' operator
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        private IQueryContainer CreateSimpleQueryUsingOr(BE.Feed feed)
        {
            QueryContainer queryContainer = null;

            queryContainer |= new TermQuery() { Field = "_id", Value = feed.Id };

            queryContainer |= new TermQuery() { Field = "name", Value = feed.Name };

            queryContainer |= new TermQuery() { Field = "linq", Value = feed.Link };

            return queryContainer;
        }

        /// <summary>
        /// Querying combining fields
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        //public List<DTO.Feed> QueryUsingCombinations(DTO.CombinedFilter filter)
        //{
        //    //Build Elastic "Should" filtering object for "Links":
        //    QueryContainer[] linksFiltering = new QueryContainer[filter.Links.Count];
        //    for (int i = 0; i < filter.Links.Count; i++)
        //    {
        //        QueryContainerDescriptor<DTO.Feed> clause = new QueryContainerDescriptor<DTO.Feed>();
        //        linksFiltering[i] = clause.Term("link", filter.Links[i]);
        //    }

        //    //Build Elastic "Must Not" filtering object for "Names":
        //    QueryContainer[] nameFiltering = new QueryContainer[filter.Names.Count];
        //    for (int i = 0; i < filter.Names.Count; i++)
        //    {
        //        QueryContainerDescriptor<DTO.Feed> clause = new QueryContainerDescriptor<DTO.Feed>();
        //        nameFiltering[i] = clause.Term("name", filter.Names[i]);
        //    }

        //    //Run the combined query:
        //    var hits = _EsClientDAL.Current.Search<DTO.Feed>(s => s.Query(q => q
        //                                                               .Queried(fq => fq
        //                                                               .Query(qq => qq.MatchAll())
        //                                                               .Filter(ff => ff
        //                                                                   .Bool(b => b
        //                                                                       .Must(m1 => m1.Term("hasChildren", filter.HasChildren))
        //                                                                       .MustNot(nameFiltering)
        //                                                                       .Should(agesFiltering)
        //                                                                   )
        //                                                                )
        //                                                             )
        //                                                          )
        //                                                        ).Hits;*/

        //    //Translate the hits and return the list
        //    List<DTO.Feed> typedList = hits.Select(hit => ConvertHitToCustumer(hit)).ToList();
        //    return typedList;
        //}

        /// <summary>
        /// Querying using ranges
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        //public List<DTO.Feed> QueryUsingRanges(DTO.RangeFilter filter)
        //{

        //    FilterContainer[] ranges = new FilterContainer[2];

        //    //Build Elastic range filtering object for "Enrollment Fee": 
        //    FilterDescriptor<DTO.Feed> clause1 = new FilterDescriptor<DTO.Feed>();
        //    ranges[0] = clause1.Range(r => r.OnField(f =>
        //                                                f.EnrollmentFee).Greater(filter.EnrollmentFeeStart)
        //                                                                .Lower(filter.EnrollmentFeeEnd));

        //    //Build Elastic range filtering object for "Birthday": 
        //    FilterDescriptor<DTO.Feed> clause2 = new FilterDescriptor<DTO.Feed>();
        //    ranges[1] = clause2.Range(r => r.OnField(f => f.Birthday)
        //                                    .Greater(filter.Birthday.ToString(DTO.Constants.BASIC_DATE)));

        //    //Run the combined query:
        //    var hits = _EsClientDAL.Current
        //                            .Search<DTO.Feed>(s => s
        //                                                   .Query(q => q
        //                                                       .Filtered(fq => fq
        //                                                       .Query(qq => qq.MatchAll())
        //                                                       .Filter(ff => ff
        //                                                           .Bool(b => b
        //                                                               .Must(ranges)
        //                                                           )
        //                                                        )
        //                                                     )
        //                                                  )
        //                                                ).Hits;


        //    //Translate the hits and return the list
        //    List<DTO.Feed> typedList = hits.Select(hit => ConvertHitToCustumer(hit)).ToList();
        //    return typedList;
        //}

        /// <summary>
        /// Getting basic aggregations 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        //public Dictionary<string, double> GetAggregations(DTO.Aggregations filter)
        //{
        //    Dictionary<string, double> list = new Dictionary<string, double>();
        //    string agg_nickname = "feed_agg";

        //    switch (filter.AggregationType)
        //    {
        //        case "Count":
        //            {
        //                ExecuteCountAggregation(filter, list, agg_nickname);
        //                break;
        //            }


        //        case "Avg":
        //            {
        //                ExecuteAvgAggregation(filter, list, agg_nickname);
        //                break;
        //            }


        //        case "Sum":
        //            {
        //                ExecuteSumAggregation(filter, list, agg_nickname);
        //                break;
        //            }

        //        case "Min":
        //            {
        //                ExecuteMinAggregation(filter, list, agg_nickname);
        //                break;
        //            }

        //        case "Max":
        //            {
        //                ExecuteMaxAggregation(filter, list, agg_nickname);
        //                break;
        //            }


        //        default:
        //            break;
        //    }


        //    return list;
        //}

        ///// <summary>
        ///// Get Sum Aggregation
        ///// </summary>
        ///// <param name="filter"></param>
        ///// <param name="list"></param>
        ///// <param name="agg_nickname"></param>
        //private void ExecuteSumAggregation(DTO.Aggregations filter, Dictionary<string, double> list, string agg_nickname)
        //{
        //    var response = _EsClientDAL.Current.Search<DTO.Feed>(s => s
        //                                                             .Aggregations(a => a
        //                                                                  .Sum(agg_nickname, st => st
        //                                                                      .Field(filter.Field)
        //                                                                        )
        //                                                                    )
        //                                                              );

        //    list.Add(filter.Field + " Sum", response.Aggs.Sum(agg_nickname).Value.Value);
        //}

        ///// <summary>
        ///// Get Avg Aggregation
        ///// </summary>
        ///// <param name="filter"></param>
        ///// <param name="list"></param>
        ///// <param name="agg_nickname"></param>
        ////private void ExecuteAvgAggregation(DTO.Aggregations filter, Dictionary<string, double> list, string agg_nickname)
        ////{
        ////    var response = _EsClientDAL.Current.Search<DTO.Feed>(s => s
        ////                                                             .Aggregations(a => a
        ////                                                                  .Average(agg_nickname, st => st
        ////                                                                      .Field(filter.Field)
        ////                                                                        )
        ////                                                                    )
        ////                                                              );

        ////    list.Add(filter.Field + " Average", response.Aggs.Average(agg_nickname).Value.Value);
        ////}

        /////// <summary>
        /////// Get Count Aggregation
        /////// </summary>
        /////// <param name="filter"></param>
        /////// <param name="list"></param>
        /////// <param name="agg_nickname"></param>
        ////private void ExecuteCountAggregation(DTO.Aggregations filter, Dictionary<string, double> list, string agg_nickname)
        ////{
        ////    var response = _EsClientDAL.Current.Search<DTO.Feed>(s => s
        ////                                                             .Aggregations(a => a
        ////                                                                  .Terms(agg_nickname, st => st
        ////                                                                      .Field(filter.Field)
        ////                                                                      .Size(int.MaxValue)
        ////                                                                      .ExecutionHint(TermsAggregationExecutionHint.GlobalOrdinals)
        ////                                                                        )
        ////                                                                    )
        ////                                                              );

        ////    foreach (var item in response.Aggs.Terms(agg_nickname).Items)
        ////    {
        ////        list.Add(item.Key, item.DocCount);
        ////    }
        ////}

        ///// <summary>
        ///// Get Min Aggregation
        ///// </summary>
        ///// <param name="filter"></param>
        ///// <param name="list"></param>
        ///// <param name="agg_nickname"></param>
        //private void ExecuteMinAggregation(DTO.Aggregations filter, Dictionary<string, double> list, string agg_nickname)
        //{
        //    var response = _EsClientDAL.Current.Search<DTO.Feed>(s => s
        //                                                             .Aggregations(a => a
        //                                                                  .Min(agg_nickname, st => st
        //                                                                      .Field(filter.Field)
        //                                                                        )
        //                                                                    )
        //                                                              );

        //    list.Add(filter.Field + " Min", response.Aggs.Sum(agg_nickname).Value.Value);
        //}

        ///// <summary>
        ///// Get Max Aggregation
        ///// </summary>
        ///// <param name="filter"></param>
        ///// <param name="list"></param>
        ///// <param name="agg_nickname"></param>
        //private void ExecuteMaxAggregation(DTO.Aggregations filter, Dictionary<string, double> list, string agg_nickname)
        //{
        //    var response = _EsClientDAL.Current.Search<DTO.Feed>(s => s
        //                                                             .Aggregations(a => a
        //                                                                  .Max(agg_nickname, st => st
        //                                                                      .Field(filter.Field)
        //                                                                        )
        //                                                                    )
        //                                                              );

        //    list.Add(filter.Field + " Max", response.Aggs.Sum(agg_nickname).Value.Value);
        //}

        #endregion
    }
}
