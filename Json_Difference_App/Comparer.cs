using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace Json_Difference_App
{
    public class Comparer
    {
        public void Compare()
        {
            string left = "";
            string right = "";
            using (StreamReader r = new StreamReader(@"../../../JSON/left.json"))
            {
                left = r.ReadToEnd();
            }
            using (StreamReader r = new StreamReader(@"../../../JSON/right.json"))
            {
                right = r.ReadToEnd();
            }

            JToken leftJson = JToken.Parse(left);
            JToken rightJson = JToken.Parse(right);

            var difference = Compare(leftJson, rightJson);

            Console.WriteLine(difference.ToString());

        }

        public JObject Compare(JToken leftJson, JToken rightJson)
        {
            var difference = new JObject();
            if (JToken.DeepEquals(leftJson, rightJson)) return difference;

            switch (leftJson.Type)
            {
                case JTokenType.Object:
                    {
                        var LeftJSON = leftJson as JObject;
                        var RightJSON = rightJson as JObject;
                        var RemovedTags = LeftJSON.Properties().Select(c => c.Name).Except(RightJSON.Properties().Select(c => c.Name));
                        var AddedTags = RightJSON.Properties().Select(c => c.Name).Except(LeftJSON.Properties().Select(c => c.Name));
                        var UnchangedTags = LeftJSON.Properties().Where(c => JToken.DeepEquals(c.Value, RightJSON[c.Name])).Select(c => c.Name);
                        foreach (var tag in RemovedTags)
                        {
                            difference[tag] = new JObject
                            {
                                ["-"] = LeftJSON[tag]
                            };
                        }
                        foreach (var tag in AddedTags)
                        {
                            difference[tag] = new JObject
                            {
                                ["-"] = RightJSON[tag]
                            };
                        }
                        var ModifiedTags = LeftJSON.Properties().Select(c => c.Name).Except(AddedTags).Except(UnchangedTags);
                        foreach (var tag in ModifiedTags)
                        {
                            var foundDifference = Compare(LeftJSON[tag], RightJSON[tag]);
                            if (foundDifference.HasValues) 
                            {
                                difference[tag] = foundDifference;
                            }
                        }
                    }
                    break;
                case JTokenType.Array:
                    {
                        var LeftArray = leftJson as JArray;
                        var RightArray = rightJson as JArray;

                        if (LeftArray != null && RightArray != null)
                        {
                            if (LeftArray.Count() == RightArray.Count())
                            {
                                for (int index=0;index<LeftArray.Count();index++)
                                {
                                    var foundDifference = Compare(LeftArray[index], RightArray[index]);
                                    if (foundDifference.HasValues)
                                    {
                                        difference[$"{index}"] = foundDifference;
                                    }
                                }
                            }
                            else
                            {
                                var left = new JArray(LeftArray.Except(RightArray, new JTokenEqualityComparer()));
                                var right = new JArray(RightArray.Except(LeftArray, new JTokenEqualityComparer()));
                                if (left.HasValues) 
                                {
                                    difference["-"] = left;
                                }
                                if (right.HasValues)
                                {
                                    difference["+"] = right;
                                }
                            }
                        }
                    }
                    break;
                default:
                    difference["-"] = leftJson;
                    difference["+"] = rightJson;
                    break;
            }

            return difference;
        }

    }
}
