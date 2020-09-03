﻿// Copyright (C) 2020 Antik Mozib. Released under GNU GPLv3.

using System;
using System.Globalization;
using System.Windows.Data;

namespace AMDownloader
{
    class DownloaderEtaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return String.Empty;
            try
            {
                double.TryParse(value.ToString(), out double remaining);
                if (remaining > 0 && !double.IsInfinity(remaining))
                {
                    TimeSpan t = TimeSpan.FromMilliseconds(remaining);
                    return t.Minutes + "m " + t.Seconds + "s";
                }
            }
            catch
            {
                return String.Empty;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
