using System.Numerics;
using Xunit;

namespace TinyEmbree.Tests {
    public class NearestNeighbor_Simple {
        [Fact]
        public void SinglePoint_ShouldBeFound() {
            //Given
            using var tree = new NearestNeighborSearch<int>();
            var p = new Vector3(14, 2.3f, 1.9f);
            tree.AddPoint(p, 13);
            tree.Build();

            //When
            var result = tree.QueryNearestSorted(Vector3.Zero, 1, float.MaxValue, out float _);

            //Then
            Assert.Equal(1, result.Length);
            Assert.Equal(13, tree.GetUserData(result[0].Id));
        }

        [Fact]
        public void SinglePoint_OutsideRadius() {
            //Given
            using var tree = new NearestNeighborSearch<int>();
            var p = new Vector3(14, 2.3f, 1.9f);
            tree.AddPoint(p, 13);
            tree.Build();

            //When
            var result = tree.QueryNearestSorted(Vector3.Zero, 1, 0.1f, out float _);

            //Then
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public void TwoPoints_OnlyClosestShouldBeFound() {
            //Given
            using var tree = new NearestNeighborSearch<int>();
            var p = new Vector3(14, 2.3f, 1.9f);
            var pfar = new Vector3(184, 2901, 231);
            tree.AddPoint(p, 13);
            tree.AddPoint(pfar, 1);
            tree.Build();

            //When
            var result = tree.QueryNearestSorted(Vector3.Zero, 1, float.MaxValue, out float _);

            //Then
            Assert.Equal(1, result.Length);
            Assert.Equal(13, tree.GetUserData(result[0].Id));
        }

        [Fact]
        public void TwoPoints_BothShouldBeFound() {
            //Given
            using var tree = new NearestNeighborSearch<int>();
            var p = new Vector3(14, 2.3f, 1.9f);
            var pfar = new Vector3(184, 2901, 231);
            tree.AddPoint(p, 13);
            tree.AddPoint(pfar, 1);
            tree.Build();

            //When
            var result = tree.QueryNearestSorted(Vector3.Zero, 2, float.MaxValue, out float _);

            //Then
            Assert.Equal(2, result.Length);
            Assert.Equal(13, tree.GetUserData(result[0].Id));
            Assert.Equal(1, tree.GetUserData(result[1].Id));
        }

        [Fact]
        public void TwoPoints_OnlyInRadiusShouldBeFound() {
            //Given
            using var tree = new NearestNeighborSearch<int>();
            var p = new Vector3(14, 2.3f, 1.9f);
            var pfar = new Vector3(184, 2901, 231);
            tree.AddPoint(p, 13);
            tree.AddPoint(pfar, 1);
            tree.Build();

            //When
            var result = tree.QueryNearestSorted(Vector3.Zero, 2, 20.0f, out float _);

            //Then
            Assert.Equal(1, result.Length);
            Assert.Equal(13, tree.GetUserData(result[0].Id));
        }

        [Fact]
        public void Clear_ShouldBeEmpty() {
            //Given
            using var tree = new NearestNeighborSearch<int>();
            var p = new Vector3(14, 2.3f, 1.9f);
            var pfar = new Vector3(184, 2901, 231);
            tree.AddPoint(p, 13);
            tree.AddPoint(pfar, 1);
            tree.Build();

            //When
            tree.Clear();
            tree.Build();
            var result = tree.QueryNearestSorted(Vector3.Zero, 2, float.MaxValue, out float _);

            //Then
            Assert.True(result.IsEmpty);
        }
    }
}