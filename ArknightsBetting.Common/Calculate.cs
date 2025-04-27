using OpenCvSharp;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ArknightsBetting.Common {
    public struct MapRect : ICloneable, IEquatable<MapRect> {
        public int SourceX { get; set; }
        public int SourceY { get; set; }
        public int TargetX { get; set; }
        public int TargetY { get; set; }

        public object Clone() {
            return new MapRect() {
                SourceX = this.SourceX,
                SourceY = this.SourceY,
                TargetX = this.TargetX,
                TargetY = this.TargetY
            };
        }

        public bool Equals(MapRect other) {
            if (SourceX == other.SourceX &&
                SourceY == other.SourceY &&
                TargetX == other.TargetX &&
                TargetY == other.TargetY) {
                return true;
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            var hashcode = new HashCode();
            hashcode.Add(SourceX);
            hashcode.Add(SourceY);
            hashcode.Add(TargetX);
            hashcode.Add(TargetY);
            return hashcode.ToHashCode();
        }
    }
    public class Calculate {
        public static int[,] GenerateMap(int[] source) {
            var map = new int[16, 10];
            for (var y = 0; y < 16; y++) {
                for (var x = 0; x < 10; x++) {
                    map[y, x] = source[y * 10 + x];
                }
            }
            return map;

        }
        public static int CalculateRectSum(MapRect rect, int[,] map) {
            var sum = 0;
            for (var x = rect.SourceX; x <= rect.TargetX; x++) {
                for (var y = rect.SourceY; y <= rect.TargetY; y++) {
                    sum += map[y, x];
                }
            }
            return sum;
        }
        public static bool IsValidRect(MapRect rect, int[,] map) {
            try {
                return CalculateRectSum(rect, map) == 10;
            } catch (System.IndexOutOfRangeException) {
                return false;
            }
        }
        static int[,] Copy2DArray(int[,] originalArray) {
            int[,] copiedArray = new int[originalArray.GetLength(0), originalArray.GetLength(1)];
            Array.Copy(originalArray, copiedArray, originalArray.Length);
            return copiedArray;
        }

        public static int[,] SetValidRect(MapRect rect, int[,] map) {
            //if (!IsValidRect(rect, map)) {
            //    throw new Exception($"Not Valid Rect{rect.SourceX},{rect.SourceY},{rect.TargetX},{rect.TargetY}");
            //}
            var tmp = Copy2DArray(map);
            for (var x = rect.SourceX; x <= rect.TargetX; x++) {
                for (var y = rect.SourceY; y <= rect.TargetY; y++) {
                    tmp[y, x] = 0;
                }
            }
            return tmp;
        }
        public static int[,] SetValidRectRaw(MapRect rect, int[,] map) {
            for (var x = rect.SourceX; x <= rect.TargetX; x++) {
                for (var y = rect.SourceY; y <= rect.TargetY; y++) {
                    map[y, x] = 0;
                }
            }
            return map;
        }
        public static int CalculateScore(int[,] map) {
            var score = 0;
            foreach(var num in map) {
                if (num == 0) {
                    score++;
                }
            }
            return score;
        }
        public static List<MapRect> GetOrCopyMapRect(List<MapRect> rects) => rects != null ? new List<MapRect>(rects.Select(item => (MapRect)item.Clone())) : new List<MapRect>();
        #region DFS
        public static (List<MapRect>, int[,]) CalculateBestScoreDFS(int[,] Map, List<MapRect> rects = null) {
            var Scores = new List<(List<MapRect>, int[,])>();
            Parallel.For(0, Map.GetLength(0), StartY => {
                for (var StartX = 0; StartX < Map.GetLength(1); StartX++) {
                    for (var EndX = StartX; EndX < Map.GetLength(1); EndX++) {
                        for (var EndY = StartY; EndY < Map.GetLength(0); EndY++) {
                            var rect = new MapRect { SourceX = StartX, SourceY = StartY, TargetX = EndX, TargetY = EndY };
                            if (IsValidRect(rect, Map)) {
                                var NewMap = SetValidRect(rect, Map);
                                List<MapRect> rectsCopy = GetOrCopyMapRect(rects);
                                Scores.Add(CalculateBestScoreDFS(NewMap, rectsCopy));
                            }
                        }
                    }
                }
            });
            //for (var StartX = 0; StartX < TmpMap.GetLength(1); StartX++) {
            //    for (var StartY = 0; StartY < TmpMap.GetLength(0); StartY++) {
            //        for (var EndX = StartX; EndX < TmpMap.GetLength(1); EndX++) {
            //            for (var EndY = StartY; EndY < TmpMap.GetLength(0); EndY++) {
            //                var rect = new MapRect { SourceX = StartX, SourceY = StartY, TargetX = EndX, TargetY = EndY };
            //                if (IsValidRect(rect, TmpMap)) {
            //                    var NewMap = SetValidRect(rect, TmpMap);
            //                    List<MapRect> rectsCopy = GetOrCopyMapRect(rects);
            //                    Scores.Add(CalculateBestScoreDFS(NewMap, rectsCopy));
            //                }
            //            }
            //        }
            //    }
            //}
            var MaxScores = (rects, Map);
            foreach (var e in Scores) {
                if (CalculateScore(e.Item2) > CalculateScore(MaxScores.Item2)) {
                    MaxScores = e;
                }
            }
            return MaxScores;
        }
        #endregion
        #region GreedyAlgorithm
        public static (List<MapRect>, int[,]) CalculateBestScoreGreedyAlgorithm(int[,] Map) {
            var TmpMap = Copy2DArray(Map);
            var Score = new List<MapRect>();
            var ScoreLength = Score.Count;
            var MaxX = TmpMap.GetLength(1);
            var MaxY = TmpMap.GetLength(0);
            var NCount = 0;
            while (true) {
                for (var MaxOffset = 0; MaxOffset < (MaxX > MaxY ? MaxX : MaxY); MaxOffset++) {
                    var MaxOffsetX = (MaxX < MaxOffset ? MaxX : MaxOffset);
                    var MaxOffsetY = (MaxY < MaxOffset ? MaxY : MaxOffset);
                    for (var StartNumber = 9; StartNumber > 0; StartNumber--) {
                        for (var CenterX = 0; CenterX < MaxX; CenterX++) {
                            for (var CenterY = 0; CenterY < MaxY; CenterY++) {
                                if (TmpMap[CenterY, CenterX] != StartNumber) {
                                    continue;
                                }
                                
                                for (var OffsetX = -MaxOffsetY; OffsetX < MaxOffsetX; OffsetX++) {
                                    if (CenterX + OffsetX < 0 || CenterX + OffsetX >= MaxX) {
                                        break;
                                    }

                                    for (var OffsetY = -MaxOffsetY; OffsetY < MaxOffsetY; OffsetY++) {
                                        if (CenterY + OffsetY < 0 || CenterY + OffsetY >= MaxY) {
                                            break;
                                        }
                                        var rect = new MapRect {
                                            SourceX = OffsetX < 0 ? CenterX + OffsetX : CenterX,
                                            SourceY = OffsetY < 0 ? CenterY + OffsetY : CenterY,
                                            TargetX = OffsetX > 0 ? CenterX + OffsetX : CenterX,
                                            TargetY = OffsetY > 0 ? CenterY + OffsetY : CenterY,
                                        };
                                        if (rect.SourceX == rect.TargetX && rect.SourceY == rect.TargetY) {
                                            break;
                                        }
                                        if (IsValidRect(rect, TmpMap)) {
                                            TmpMap = SetValidRectRaw(rect, TmpMap);
                                            Score.Add(rect);
                                            //goto NextPoint;
                                        }
                                    }
                                }
                            NextPoint:
                                continue;
                            }
                        }
                    NextSearch:
                        continue;
                    }
                }

                if (Score.Count == ScoreLength) {
                    NCount++;
                } 
                if (NCount > 3) {
                    break;
                } else {
                    ScoreLength = Score.Count;
                }
            }
            var OutScore = CalculateScore(TmpMap);
            Console.WriteLine(OutScore);
            return (Score, TmpMap);
        }
        #endregion
        #region ACO
        public struct MapRoute : IEquatable<MapRoute> {
            public MapRect From { get; set; }
            public MapRect To { get; set; }

            public bool Equals(MapRoute other) {
                if (From.Equals(other.From) && To.Equals(other.To)) { 
                    return true; 
                } else {
                    return false;
                }
            }
            public override int GetHashCode() {
                var hashcode = new HashCode();
                hashcode.Add(From.GetHashCode());
                hashcode.Add(To.GetHashCode());
                return hashcode.ToHashCode();
            }
        }
        public class Ant {
            public static Dictionary<MapRoute, double> Pheromones { get; set; } = new Dictionary<MapRoute, double>();
            public static int BestScore { get; set; } = 0;
            public static List<MapRect> BestRoute { get; set; } = new List<MapRect>();
            public static int[,] BestMap { get; set; } = new int[,] { };
            public int[,] Map { get; set; }
            public Random Rnd { get; set; }
            public MapRect Previous { get; set; }
            public List<MapRect> Route { get; set; }
            public Ant(int[,] map) {
                Map = Copy2DArray(map);
                Rnd = new Random();
                Previous = new MapRect() {
                    SourceX = -1,
                    SourceY = -1,
                    TargetX = -1,
                    TargetY = -1,
                };
                Route = new List<MapRect>();
            }
            public static void ReInit() {
                Pheromones = new Dictionary<MapRoute, double>();
                BestScore = 0;
                BestRoute = new List<MapRect>();
                BestMap = new int[,] { };
            }
            public bool GetCurrentSolutions(out List<MapRect> solutions) {
                solutions = GetCurrentSolutions();
                return solutions.Count > 0;
            }
            public List<MapRect> GetCurrentSolutions() {
                var Solutions = new List<MapRect>();
                for (int SourceY = 0; SourceY < Map.GetLength(0); SourceY++) {
                    for (int SourceX = 0; SourceX < Map.GetLength(1); SourceX++) {
                        for (int TargetY = 0; TargetY < Map.GetLength(0); TargetY++) {
                            for (int TargetX = 0; TargetX < Map.GetLength(1); TargetX++) {
                                var rect = new MapRect {
                                    SourceX = SourceX,
                                    SourceY = SourceY,
                                    TargetX = TargetX,
                                    TargetY = TargetY,
                                };
                                if (IsValidRect(rect, Map)) {
                                    Solutions.Add(rect);
                                }
                            }
                        }
                    }
                }
                return Solutions;
            }
            public MapRect SelectSolutions(List<MapRect> Solutions, bool IsFirst = false) {
                
                var count = 0.0;
                if (IsFirst) {
                    count = Solutions.Count;
                } else {
                    for (var i = 0; i < Solutions.Count; i++) {
                        if (Pheromones.TryGetValue(new MapRoute() { From = Previous, To = Solutions[i] }, out var value)) {
                            count += value;
                        } else {
                            count += 1;
                        }
                    }
                }
                var rand = Rnd.NextDouble() * count;
                var currentValue = 0.0;
                for (var i = 0; i < Solutions.Count; i++) {
                    if (Pheromones.TryGetValue(new MapRoute() { From = Previous, To = Solutions[i] }, out var value)) {
                        if (currentValue < rand && currentValue + value > rand) {
                            return Solutions[i];
                        } else if (IsFirst) {
                            currentValue += 1;
                        } else {
                            currentValue += value;
                        }
                    } else {
                        if (currentValue < rand && currentValue + 1 > rand) {
                            return Solutions[i];
                        } else {
                            currentValue += 1;
                        }
                    }
                }
                throw new Exception("Not Found");
            }

            public void UpadtePheromones() {
                var score = CalculateScore(Map);
                if (score > BestScore) {
                    lock (BestRoute) {
                        lock (BestMap) {
                            BestScore = score;
                            BestMap = Map;
                            BestRoute = Route;
                        }
                    }
                }
                for (var i = 0; i < Route.Count; i++) {
                    MapRoute perRoute;
                    if (i == 0) {
                        perRoute = new MapRoute() {
                            From = new MapRect() { SourceX = -1, SourceY = -1, TargetX = -1, TargetY = -1 },
                            To = Route[i]
                        };
                        
                    } else {
                        perRoute = new MapRoute() {
                            From = Route[i - 1],
                            To = Route[i],
                        };
                    }
                    lock (Pheromones) {
                        if (Pheromones.ContainsKey(perRoute)) {
                            Pheromones[perRoute] += score;
                        } else {
                            Pheromones.Add(perRoute, score);
                        }
                    }
                }
            }

            public void Run() {
                var IsFirst = true;
                while (GetCurrentSolutions(out var solutions)) {
                    var rect = SelectSolutions(solutions, IsFirst);
                    IsFirst = false;
                    Map = SetValidRectRaw(rect, Map);
                    Route.Add(rect);
                }
                UpadtePheromones();
            }
        }
        public static (List<MapRect>, int[,]) CalculateBestScoreACO(int[,] Map) {
            Ant.ReInit();
            var currentMaxScore = 0;
            var notUpdateMaxScore = 0;
            var startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMinutes < 1) {
                Parallel.For(0, Environment.ProcessorCount, (body) => {
                    var ant = new Ant(Map);
                    ant.Run();
                    if (currentMaxScore < Ant.BestScore) {
                        currentMaxScore = Ant.BestScore;
                        notUpdateMaxScore = 0;
                    } else {
                        notUpdateMaxScore++;
                    }
                });
                Parallel.ForEach(Ant.Pheromones.Keys, (e) => {
                    if (Ant.Pheromones[e] > 5) {
                        Ant.Pheromones[e] -= 5;
                    } else {
                        Ant.Pheromones[e] = 1;
                    }
                });
            }
            return (Ant.BestRoute, Ant.BestMap);
        }
        #endregion
    }
}
