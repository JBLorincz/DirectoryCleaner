using System.Text.RegularExpressions;

namespace DirectoryCleaner
{
    /// <summary>
    /// determines wether an object in the source directory is junk or not.
    /// </summary>
    public class JunkIdentifier
    {
        private readonly string timeToLookBack;
        public JunkIdentifier(string timeToLookBack)
        {
            this.timeToLookBack = timeToLookBack;
        }
        public bool IsJunkViaLastAccessed()
        {
            throw new NotImplementedException();
        }
        public bool IsJunkViaLastModified()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts something like "1w" into a date time object one week away from current time
        /// </summary>
        /// <returns></returns>
        public TimeSpan DateOffsetToTimespan()
        {
            string pattern = @"\d+[mdwh]";
            Regex r = new(pattern);

            MatchCollection parseableTokens = r.Matches(timeToLookBack);

            if (parseableTokens.Count < 1) throw new InvalidOperationException("Cannot understand the timestring provided.");

            int months = 0, weeks = 0, days = 0, hours = 0;
            foreach (Match match in parseableTokens)
            {
                string precedent = match.Value.Substring(0, match.Length - 1);
                int value = int.Parse(precedent); //handle potential error
                char endOfToken = char.ToLower(match.Value.Last());
                switch (endOfToken)
                {
                    case 'w':
                        weeks += value;
                        break;
                    case 'd':
                        days += value;
                        break;
                    case 'm':
                        months += value;
                        break;
                    case 'h':
                        hours += value;
                        break;

                }

            }
            return TimeSpan.FromHours(hours) + TimeSpan.FromDays(days) + TimeSpan.FromDays(7 * weeks) + TimeSpan.FromDays(30 * months);
        }
    }
}
