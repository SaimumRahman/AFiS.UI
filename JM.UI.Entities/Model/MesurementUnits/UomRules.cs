using System;
using System.Collections.Generic;

namespace JM.UI.Entities.Model.MesurementUnits
{
    /// <summary>
    /// Quantity rules derived from an item's measurement unit (UoM).
    /// Length-based units — Yards and Meter and their common spellings — may be
    /// entered with decimals; every other unit is whole units only.
    /// </summary>
    public static class UomRules
    {
        /// <summary>
        /// Unit names that allow a fractional quantity. Matched case-insensitively
        /// after trimming, so "Yards", "YARDS" and " Yd " all qualify.
        /// </summary>
        private static readonly HashSet<string> DecimalUnitNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "YARD", "YARDS", "YD", "YDS",
            "METER", "METERS", "METRE", "METRES", "MTR"
        };

        /// <summary>True when the unit allows a fractional (decimal) quantity.</summary>
        public static bool AllowsDecimalQuantity(string? uomName)
            => !string.IsNullOrWhiteSpace(uomName) && DecimalUnitNames.Contains(uomName.Trim());

        /// <summary>
        /// Step for a quantity input: "0.01" when decimals are allowed, otherwise "1".
        /// RadzenNumeric.Step is a string (it lands on the HTML step attribute), so the
        /// value is returned already formatted.
        /// </summary>
        public static string StepFor(bool allowsDecimal)
            => allowsDecimal ? "0.01" : "1";

        /// <summary>Step for a quantity input, decided by the unit name.</summary>
        public static string StepFor(string? uomName)
            => StepFor(AllowsDecimalQuantity(uomName));

        /// <summary>Format for a quantity display: "0.##" trims trailing zeros, "0" forces a whole number.</summary>
        public static string FormatFor(bool allowsDecimal)
            => allowsDecimal ? "0.##" : "0";

        /// <summary>Format for a quantity display, decided by the unit name.</summary>
        public static string FormatFor(string? uomName)
            => FormatFor(AllowsDecimalQuantity(uomName));

        /// <summary>
        /// Rounds a quantity to a whole number when decimals are not allowed.
        /// Returns true when the value was changed.
        /// </summary>
        public static bool Normalize(bool allowsDecimal, ref decimal quantity)
        {
            if (allowsDecimal)
                return false;

            var rounded = Math.Round(quantity, 0, MidpointRounding.AwayFromZero);
            if (rounded == quantity)
                return false;

            quantity = rounded;
            return true;
        }

        /// <summary>
        /// Rounds a quantity to a whole number when the unit does not allow decimals.
        /// Returns true when the value was changed.
        /// </summary>
        public static bool Normalize(string? uomName, ref decimal quantity)
            => Normalize(AllowsDecimalQuantity(uomName), ref quantity);
    }
}
