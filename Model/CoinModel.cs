using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tryWeb.Model
{
    public class CoinModel
    {
        public ForTether tether { get; set; }
        public ForBitcoin bitcoin { get; set; }
        public ForRipple ripple { get; set; }
    }

    public class ForTether
    {
        public double uah { get; set; }
    }
    public class ForBitcoin
    {
        public double uah { get; set; }
    }
    public class ForRipple
    {
        public double uah { get; set; }
    }
}
