using HorseFeederAvalonia.Enums;
using System;

namespace HorseFeederAvalonia.Models
{
    public class SlotConfiguration
    {
        public int Slot { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public RepetitionFrequency? RepetitionFrequency { get; set; }
    }
}
