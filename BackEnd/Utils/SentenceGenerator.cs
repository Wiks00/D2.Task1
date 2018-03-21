using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentenceGenerator
{
    public static class Generator
    {
        private static string[][] words =
        {
            new [] {"the", "a", "one", "some", "any",},
            new [] {"boy", "girl", "dog", "town", "car",},
            new [] {"drove", "jumped", "ran", "walked", "skipped",},
            new [] {"to", "from", "over", "under", "on",}
        };

        public static string GenerateMessage()
        {
            Random rdm = new Random(DateTimeOffset.UtcNow.Millisecond + DateTime.Now.Second);

            List<int> randomNumbers = Enumerable.Repeat(rdm.Next(1, 5), rdm.Next(1, 4)).ToList();

            return string.Join(" ", randomNumbers.Select((item, i) => words[i][item]));
        }
    }
}
