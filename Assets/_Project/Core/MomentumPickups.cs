using System.Collections.Generic;

namespace IncrementalGame.Core
{
    public sealed class MomentumPickup
    {
        public const double Radius=18;
        public int Id;
        public SimVector2 Position;
        public bool Active=true;
    }
    public sealed partial class MomentumSimulation
    {
        public const int BountyReward=12;
        public readonly List<MomentumPickup> Pickups=new List<MomentumPickup>();
        private void ResetPickups()
        {
            Pickups.Clear();
            Pickups.Add(new MomentumPickup { Id=1,Position=new SimVector2(660,620) });
            Pickups.Add(new MomentumPickup { Id=2,Position=new SimVector2(940,620) });
        }
        private void RedeemBounty(MomentumBall ball)
        {
            ball.BountyCharged=false;
            if(Progress==null) return;
            Progress.gold+=BountyReward;ChallengeGold+=BountyReward;
            Events.Add(new MomentumEvent("bounty",ball.Position,BountyReward));
        }
    }
}
