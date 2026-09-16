using UnityEngine;

namespace MazeGame.Generation
{
    public static class MazeDifficultyCurve
    {
        public static void Apply(MazeConfig cfg, int level)
        {
            level = Mathf.Clamp(level, 1, 10);
            float t = (level - 1) / 9f;

            cfg.rows = Mathf.RoundToInt(Mathf.Lerp(5, 12, t));
            cfg.cols = Mathf.RoundToInt(Mathf.Lerp(5, 12, t));
            cfg.targetPathLength = Mathf.RoundToInt(Mathf.Lerp(8, 40, t));

            // Braided loops only kick in for the last few levels - false paths, not just longer ones
            cfg.loopChance = level >= 8 ? Mathf.Lerp(0.05f, 0.15f, (level - 8) / 2f) : 0f;
        }
    }
}
