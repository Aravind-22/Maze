# Maze Game

Procedurally generated maze with custom player movement and collision, built in Unity.

## Features
- Maze generation via Kruskal's algorithm using a Union-Find (disjoint-set) data structure
- Substepped custom movement/collision for the player (same pattern as Hoop Strike)
- World-space-driven object hierarchy — ball/player positioned as a sibling of MazeRoot, not a child, to keep positional logic explicit

## Why this approach
Kruskal's + Union-Find guarantees a perfect maze (no loops, fully connected) with clean, efficient generation logic — a deliberate DSA-to-gameplay application rather than using a prebuilt maze asset.

## Stack
Unity, C#, custom substepped physics
