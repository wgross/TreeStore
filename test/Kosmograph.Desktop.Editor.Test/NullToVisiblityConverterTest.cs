//using System;
//using System.Windows;
//using Xunit;

//namespace Kosmograph.Desktop.Test
//{
//    public class NullToVisiblityConverterTest
//    {
//        private readonly NullToVisiblityConverter converter;

//        public NullToVisiblityConverterTest()
//        {
//            this.converter = new NullToVisiblityConverter();
//        }

//        [Theory]
//        [InlineData(null, Visibility.Hidden)]
//        [InlineData((object)1, Visibility.Visible)]
//        public void Convert_to_Visibility(object value, Visibility expected)
//        {
//            // ACT

//            var result = this.converter.Convert(value, typeof(Visibility), null, null);

//            // ASSERT

//            Assert.Equal(expected, result);
//        }
//    }
//}