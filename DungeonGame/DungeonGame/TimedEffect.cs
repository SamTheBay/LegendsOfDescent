using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{


    public interface ITimedEffect : IActiveIcon
    {
        bool UpdateDuration(GameTime gameTime);
    }


    public class PoisonEffect : ITimedEffect
    {
        CharacterSprite owner;
        CharacterSprite target;
        int damage;
        int lastElapsed = 0;
        Timer timer;
        Rectangle source = new Rectangle((31 % 10) * 48, (31 / 10) * 48, 48, 48);

        public PoisonEffect(CharacterSprite owner, CharacterSprite target, int duration, int damage)
        {
            this.owner = owner;
            this.target = target;
            this.damage = damage;
            timer = new Timer(duration, TimerState.Running, TimerType.Manual);
        }

        public Texture2D GetActiveIconTexture()
        {
            return InternalContentManager.GetTexture("AbilityIcons");
        }


        public Rectangle GetActiveIconSource()
        {
            return source;
        }


        public int GetActiveIconTime()
        {
            return timer.RemainingDuration;
        }


        public bool IsEqualActiveIcon(IActiveIcon other)
        {
            return other is PoisonEffect;
        }


        public IActiveIcon CombineActiveIcons(IActiveIcon other)
        {
            PoisonEffect o = other as PoisonEffect;
            if (o != null)
            {
                damage = (damage * GetActiveIconTime() + o.damage * o.GetActiveIconTime()) / (GetActiveIconTime() + o.GetActiveIconTime());
                timer.AddDuration(other.GetActiveIconTime());
            }

            return this;
        }


        public bool IsDebuff()
        {
            return true;
        }

        public bool UpdateDuration(GameTime gameTime)
        {
            if (target.IsDead)
            {
                timer.State = TimerState.Stopped;
                return false;
            }

            bool result = timer.Update(gameTime);

            if (timer.Elapsed / 1000 != lastElapsed)
            {
                ParticleSystem.AddParticles(target.CenteredPosition, ParticleType.ExplosionWhite, color: Color.Green, numParticlesScale: 1f);
                target.Damage(damage, DamageType.Poison, owner);

            }
            lastElapsed = timer.Elapsed / 1000;

            return result;
        }
    }



    class BleedEffect : ITimedEffect
    {
        CharacterSprite owner;
        CharacterSprite target;
        int damage;
        int lastElapsed = 0;
        Timer timer;
        Rectangle source = new Rectangle((26 % 10) * 48, (26 / 10) * 48, 48, 48);

        public BleedEffect(CharacterSprite owner, CharacterSprite target, int duration, int damage)
        {
            this.owner = owner;
            this.target = target;
            this.damage = damage;
            timer = new Timer(duration, TimerState.Running, TimerType.Manual);
        }

        public Texture2D GetActiveIconTexture()
        {
            return InternalContentManager.GetTexture("AbilityIcons");
        }


        public Rectangle GetActiveIconSource()
        {
            return source;
        }


        public int GetActiveIconTime()
        {
            return timer.RemainingDuration;
        }


        public bool IsEqualActiveIcon(IActiveIcon other)
        {
            return other is BleedEffect;
        }

        public bool IsDebuff()
        {
            return true;
        }

        public IActiveIcon CombineActiveIcons(IActiveIcon other)
        {
            BleedEffect o = other as BleedEffect;
            if (o != null)
            {
                damage = (damage * GetActiveIconTime() + o.damage * o.GetActiveIconTime()) / (GetActiveIconTime() + o.GetActiveIconTime());
                timer.AddDuration(other.GetActiveIconTime());
            }

            return this;
        }

        public bool UpdateDuration(GameTime gameTime)
        {
            if (target.IsDead)
            {
                timer.State = TimerState.Stopped;
                return false;
            }

            bool result = timer.Update(gameTime);

            if (timer.Elapsed / 1000 != lastElapsed)
            {
                target.Damage(damage, DamageType.Physical, owner);
                ParticleSystem.AddParticles(target.CenteredPosition, ParticleType.Starburst, color: Color.Red, numParticlesScale: 1f);
            }
            lastElapsed = timer.Elapsed / 1000;

            return result;
        }
    }




    class BurnEffect : ITimedEffect
    {
        CharacterSprite owner;
        CharacterSprite target;
        int damage;
        int lastElapsed = 0;
        Timer timer;
        Rectangle source = new Rectangle((0 % 10) * 48, (0 / 10) * 48, 48, 48);

        public BurnEffect(CharacterSprite owner, CharacterSprite target, int duration, int damage)
        {
            this.owner = owner;
            this.target = target;
            this.damage = damage;
            timer = new Timer(duration, TimerState.Running, TimerType.Manual);
        }

        public Texture2D GetActiveIconTexture()
        {
            return InternalContentManager.GetTexture("AbilityIcons");
        }


        public Rectangle GetActiveIconSource()
        {
            return source;
        }


        public int GetActiveIconTime()
        {
            return timer.RemainingDuration;
        }


        public bool IsEqualActiveIcon(IActiveIcon other)
        {
            return other is BurnEffect;
        }

        public bool IsDebuff()
        {
            return true;
        }

        public IActiveIcon CombineActiveIcons(IActiveIcon other)
        {
            BurnEffect o = other as BurnEffect;
            if (o != null)
            {
                damage = (damage * GetActiveIconTime() + o.damage * o.GetActiveIconTime()) / (GetActiveIconTime() + o.GetActiveIconTime());
                timer.AddDuration(other.GetActiveIconTime());
            }

            return this;
        }

        public bool UpdateDuration(GameTime gameTime)
        {
            if (target.IsDead)
            {
                timer.State = TimerState.Stopped;
                return false;
            }

            bool result = timer.Update(gameTime);

            if (timer.Elapsed / 1000 != lastElapsed)
            {
                target.Damage(damage, DamageType.Fire, owner);
                ParticleSystem.AddParticles(target.CenteredPosition, ParticleType.Explosion, color: Color.White, numParticlesScale: 1f);
            }
            lastElapsed = timer.Elapsed / 1000;

            return result;
        }
    }




    class SlowEffect : ITimedEffect
    {
        CharacterSprite owner;
        CharacterSprite target;
        float amount;
        Timer timer;
        Rectangle source = new Rectangle((29 % 10) * 48, (29 / 10) * 48, 48, 48);

        public SlowEffect(CharacterSprite owner, CharacterSprite target, int duration, float amount)
        {
            this.owner = owner;
            this.target = target;
            this.amount = amount;
            timer = new Timer(duration, TimerState.Running, TimerType.Manual);
        }

        public Texture2D GetActiveIconTexture()
        {
            return InternalContentManager.GetTexture("AbilityIcons");
        }


        public Rectangle GetActiveIconSource()
        {
            return source;
        }


        public int GetActiveIconTime()
        {
            return timer.RemainingDuration;
        }


        public bool IsEqualActiveIcon(IActiveIcon other)
        {
            return other is SlowEffect;
        }




        public bool IsDebuff()
        {
            return true;
        }


        public float SlowAmount
        {
            get { return amount; }
        }

        public IActiveIcon CombineActiveIcons(IActiveIcon other)
        {
            SlowEffect o = other as SlowEffect;
            if (o != null)
            {
                amount = (amount * GetActiveIconTime() + o.amount * o.GetActiveIconTime()) / (GetActiveIconTime() + o.GetActiveIconTime());
                timer.AddDuration(other.GetActiveIconTime());
            }

            return this;
        }

        public bool UpdateDuration(GameTime gameTime)
        {
            if (target.IsDead)
            {
                timer.State = TimerState.Stopped;
                return false;
            }

            return timer.Update(gameTime);
        }
    }



    class StunEffect : ITimedEffect
    {
        CharacterSprite owner;
        CharacterSprite target;
        Timer timer;
        Rectangle source = new Rectangle((13 % 10) * 48, (13 / 10) * 48, 48, 48);

        public StunEffect(CharacterSprite owner, CharacterSprite target, int duration)
        {
            this.owner = owner;
            this.target = target;
            timer = new Timer(duration, TimerState.Running, TimerType.Manual);
        }

        public Texture2D GetActiveIconTexture()
        {
            return InternalContentManager.GetTexture("AbilityIcons");
        }


        public Rectangle GetActiveIconSource()
        {
            return source;
        }


        public int GetActiveIconTime()
        {
            return timer.RemainingDuration;
        }


        public bool IsEqualActiveIcon(IActiveIcon other)
        {
            return other is StunEffect;
        }

        public bool IsDebuff()
        {
            return true;
        }

        public IActiveIcon CombineActiveIcons(IActiveIcon other)
        {
            StunEffect o = other as StunEffect;
            if (o != null)
            {
                if (o.GetActiveIconTime() > GetActiveIconTime())
                {
                    timer.AddDuration(o.GetActiveIconTime() - GetActiveIconTime());
                }
            }

            return this;
        }


        public bool UpdateDuration(GameTime gameTime)
        {
            if (target.IsDead)
            {
                timer.State = TimerState.Stopped;
                return false;
            }

            ParticleSystem.AddParticles(target.CenteredPosition, ParticleType.Fountain, color: Color.Gray, numParticlesScale: .01f, lifetimeScale: .4f);

            return timer.Update(gameTime);
        }
    }


    class FearEffect : ITimedEffect
    {
        CharacterSprite owner;
        CharacterSprite target;
        Timer timer;
        Rectangle source = new Rectangle((11 % 10) * 48, (11 / 10) * 48, 48, 48);

        public FearEffect(CharacterSprite owner, CharacterSprite target, int duration)
        {
            this.owner = owner;
            this.target = target;
            timer = new Timer(duration, TimerState.Running, TimerType.Manual);
        }

        public Texture2D GetActiveIconTexture()
        {
            return InternalContentManager.GetTexture("AbilityIcons");
        }


        public Rectangle GetActiveIconSource()
        {
            return source;
        }


        public int GetActiveIconTime()
        {
            return timer.RemainingDuration;
        }


        public bool IsEqualActiveIcon(IActiveIcon other)
        {
            return other is FearEffect;
        }

        public bool IsDebuff()
        {
            return true;
        }

        public IActiveIcon CombineActiveIcons(IActiveIcon other)
        {
            FearEffect o = other as FearEffect;
            if (o != null)
            {
                if (o.GetActiveIconTime() > GetActiveIconTime())
                {
                    timer.AddDuration(o.GetActiveIconTime() - GetActiveIconTime());
                }
            }

            return this;
        }


        public bool UpdateDuration(GameTime gameTime)
        {
            if (target.IsDead)
            {
                timer.State = TimerState.Stopped;
                return false;
            }

            bool result = timer.Update(gameTime);

            if (result == true)
            {
                target.FearComplete();
            }

            return result;
        }
    }

}
