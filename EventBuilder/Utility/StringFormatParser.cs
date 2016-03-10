using System.Linq;
using System.Text.RegularExpressions;

namespace EventBuilder.Utility
{
  /* example:
   * 
   * const string input = "{0} and {1} and {0} and {4} {{5}} and {{{6:MM-dd-yyyy}}} and {{{{7:#,##0}}}} and {{{{{8}}}}}";
   * 
   * total usages = 6
   * unique parameter = 5
   * max(required) parameter count = 9
   * */
  public static class StringFormatParser
  {
    const string Pattern = @"(?<!\{)(?>\{\{)*\{\d(.*?)";



    public static int GetRequiredParameterCount(string format)
    {
      if(string.IsNullOrEmpty(format))
      {
        return 0;
      }
      var matches = Regex.Matches(format, Pattern);
      int parameterMatchCount = 0;
      if (matches.Count > 0)
      {
        parameterMatchCount =
          matches.OfType<Match>().Select(m => m.Value).Distinct().Select(m => int.Parse(m.Replace("{", string.Empty))).
            Max() + 1;
      }
      return parameterMatchCount;
    }

    public static int GetUniqueParameterCount(string format)
    {
      if (string.IsNullOrEmpty(format))
      {
        return 0;
      }
      var matches = Regex.Matches(format, Pattern);
      var uniqueMatchCount = matches.OfType<Match>().Select(m => m.Value).Distinct().Count();
      return uniqueMatchCount;
    }
  }
}
