namespace Fly.Blackbox.Console.Riff
{
    public struct _AviIndex
    {
        public string Identifier;    /* Chunk identifier reference */
        public int Flags;         /* Type of chunk referenced */
        public int Offset;        /* Position of chunk in file */
        public int Length;        /* Length of chunk in bytes */
    }
}
