using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Injector
{
    public class Header
    {
        private static readonly Dictionary<string, ConsoleColor> logo = new Dictionary<string, ConsoleColor>();
        private static bool isInit;
        private static readonly ConsoleColor color = ConsoleColor.Cyan;

        public static void Init()
        {
            isInit = true;
            logo.Add(@"  ______    _  _", color);
            logo.Add(@" /      | _| || |_                      .--.", color);
            logo.Add(@"|  ,----'|_  __  _|            ,-.------+-.|  ,-.", color);
            logo.Add(@"|  |      _| || |_    ,--=======* )""("""")===)===* )", color);
            logo.Add(@"|  `----.|_  __  _|   �        `-""---==-+-""|  `-""", color);
            logo.Add(@" \______|  |_||_|     O                 '--'", color);
            logo.Add($" [miltinh0c] (v{Assembly.GetExecutingAssembly().GetName().Version})\n", ConsoleColor.White);
        }

        public static void Draw()
        {
            if (!isInit)
            {
                Init();
            }
            Console.BackgroundColor = ConsoleColor.Black;
            ConsoleColor startColor = ConsoleColor.White;

            foreach (KeyValuePair<string, ConsoleColor> keyValue in logo)
            {
                if (Console.ForegroundColor != keyValue.Value)
                    Console.ForegroundColor = keyValue.Value;

                Console.WriteLine(keyValue.Key);
            }

            Console.ForegroundColor = startColor;
        }
    }
}
