using System;
using System.Threading.Tasks;

namespace Server.Model
{
    public sealed class CircleObject : IObject
    {
        private static readonly Random Rnd = new ();

        public CircleObject(int id)
        {
            X = Rnd.Next(1000);
            Y = Rnd.Next(1000);
            Dx = Rnd.Next(5);
            Dy = Rnd.Next(5);
            Id = id;
        }
        private int Id { get; init; }
        private int X { get; set; }
        private int Y { get; set; }
        private int Dx { get; set; }
        private int Dy { get; set; }
        public Task<bool> UpdateAsync()
        {
            X += Dx;
            Y += Dy;
            return Task.FromResult(true);
        }

        public ObjectState ToObjectState()
        {
            return new ObjectState()
            {
                Id = Id,
                X = X,
                Y = Y,
                State = "state",
                Type = "circle",
            };
        }

        public override string ToString()
        {
            return $"X = {X}; Y = {Y}";
        }
    }
}