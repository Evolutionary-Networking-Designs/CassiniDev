using System.Runtime.Serialization;

namespace HttpLibArticleSite
{
    /// <summary>
    /// A simple class to return results
    /// </summary>
    [DataContract]
    public class Result
    {
     //var x = new { Message = default(string), Session = default(string), Value = default(string) };
        

        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string Session { get; set; }
        [DataMember]
        public string Value { get; set; }
    }
}