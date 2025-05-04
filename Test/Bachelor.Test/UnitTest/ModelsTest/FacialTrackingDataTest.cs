using Bachelor.Models;
using System;
using Xunit;

namespace Bachelor.Test
{
    public class FacialTrackingDataTests
    {
        [Fact]
        public void LeftEyebrowHeight_CalculatesCorrectly()
        {
             var data = new FacialTrackingData
            {
                lEyebrowY = 10.0,
                lEyesocketY = 5.0
            };

             double result = data.LeftEyebrowHeight;

            Assert.Equal(5.0, result);
        }

        [Fact]
        public void RightEyebrowHeight_CalculatesCorrectly()
        {
             var data = new FacialTrackingData
            {
                rEyebrowY = 12.0,
                rEyesocketY = 8.0
            };

             double result = data.RightEyebrowHeight;

             Assert.Equal(4.0, result);
        }

        [Fact]
        public void MouthHeight_CalculatesCorrectly()
        {
             var data = new FacialTrackingData
            {
                MouthBotY = 20.0,
                MouthTopY = 15.0
            };

             double result = data.MouthHeight;

             Assert.Equal(5.0, result);
        }

        [Fact]
        public void MouthWidth_CalculatesCorrectly()
        {
             var data = new FacialTrackingData
            {
                MouthLX = 10.0,
                MouthRX = 5.0
            };

             double result = data.MouthWidth;

             Assert.Equal(5.0, result);
        }

        [Fact]
        public void HeadPitch_CalculatesCorrectly()
        {
             var data = new FacialTrackingData
            {
                ForeheadZ = 10.0,
                ChinZ = 20.0,
                rEyeCornerZ = 5.0,
                lEyeCornerZ = 15.0
            };

             double result = data.HeadPitch;

             Assert.Equal(5.0, result); // (10+20)/2 - (5+15)/2 = 15 - 10 = 5
        }

        [Fact]
        public void CalculateHeadTiltAngle_ReturnsCorrectAngle()
        {
             var data = new FacialTrackingData
            {
                lEyeCornerY = 2.0,
                rEyeCornerY = 1.0
            };

             double result = data.CalculateHeadTiltAngle();

            // Expected: arctan(1/1) in degrees = 45 degrees
            Assert.Equal(45.0, result, 0.01); // Using precision for floating-point comparison
        }

        [Fact]
        public void SelectiveHeadRotation_ReturnsCorrectAngle()
        {
             var data = new FacialTrackingData
            {
                lEarZ = 2.0,
                rEarZ = 1.0
            };

            double result = data.SelectiveHeadRotation();

            // Expected: arctan(1/1) in degrees = 45 degrees
            Assert.Equal(45.0, result, 0.01); // Using precision for floating-point comparison
        }
    }
}