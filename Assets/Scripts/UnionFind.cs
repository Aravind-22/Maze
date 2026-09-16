namespace MazeGame.Generation
{
    public class UnionFind
    {
        private readonly int[] parent;
        private readonly int[] rank;

        public UnionFind(int size)
        {
            parent = new int[size];
            rank = new int[size];
            for (int i = 0; i < size; i++) parent[i] = i;
        }

        public int Find(int x)
        {
            if (parent[x] != x) parent[x] = Find(parent[x]); // path compression
            return parent[x];
        }

        public bool Union(int a, int b)
        {
            int ra = Find(a);
            int rb = Find(b);
            if (ra == rb) return false;

            if (rank[ra] < rank[rb]) (ra, rb) = (rb, ra);
            parent[rb] = ra;
            if (rank[ra] == rank[rb]) rank[ra]++;
            return true;
        }
    }
}
