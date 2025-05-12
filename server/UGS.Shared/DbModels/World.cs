using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGS.Shared.DbModels
{
    public class World
    {
        public int Id { get; set; }
        public required GameSpec Game { get; set; }
        public string Name { get; set; }
    }
}
