using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class FrozenGroundAbility : Ability
    {

        int[] damage = { 1, 3, 6, 15, 30, 50 };
        Timer tileTimer = new Timer(10000, TimerState.Stopped, TimerType.Manual);
        List<TileSprite> tiles = new List<TileSprite>();
        List<EnemySprite> alreadyDamagedEnemies = new List<EnemySprite>();
        Timer damageEnemiesTimer = new Timer(1000, TimerState.Running, TimerType.Auto);
        int[] slowTime = { 5000, 6000, 7000, 8000, 9000, 10000 };
        int[] radius = {1, 2, 3, 4, 5, 6};
        int attackDuration = 1000;

        public FrozenGroundAbility(PlayerSprite player)
            : base("Frozen Ground", player, 22)
        {
            manaCost[0] = 30;
            manaCost[1] = 40;
            manaCost[2] = 50;
            manaCost[3] = 60;
            manaCost[4] = 70;
            manaCost[5] = 80;
            elapsed = duration = 30000;
            isMagic = true;
            abilityType = AbilityType.FrozenGround;

            abilityPointCost[0] = 5;
            abilityPointCost[1] = 15;
            abilityPointCost[2] = 45;
            abilityPointCost[3] = 120;
            abilityPointCost[4] = 350;
            abilityPointCost[5] = 1000;

            requiredAbilityType = AbilityType.IceBolt;
            requiredLevel = 2;
        }


        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.Mana >= manaCost[level - 1])
            {
                // adjust speed of attack animation for this spell
                float durAdjust = (480f / ((float)attackDuration / (player.CastSpeedAdjust + 1f))) - 1f;
                player.SetAttackAnimationInterval(durAdjust);
                player.UseMana(manaCost[level - 1]);

                // reset timer
                base.Activate(direction);
                activateDelayElapsed = 0;
                elapsed = 0;
                AudioManager.audioManager.PlaySFX("Magic" + Util.Random.Next(1, 6).ToString());
                return true;
            }
            return false;
        }


        public override void ActivateFinish(Vector2 direction)
        {
            // select tiles
            TileSprite centerTile = GameplayScreen.Dungeon.GetTileAtPosition(direction);

            for (int x = centerTile.TileLocation.X - radius[level - 1]; x <= centerTile.TileLocation.X + radius[level - 1]; x++)
            {
                for (int y = centerTile.TileLocation.Y - radius[level - 1]; y <= centerTile.TileLocation.Y + radius[level - 1]; y++)
                {
                    TileSprite tile = GameplayScreen.Dungeon.GetTile(x, y);

                    if (Vector2.Distance(new Vector2(x, y), centerTile.TileLocation.ToVector2()) > radius[level - 1])
                    {
                        continue;
                    }

                    if (tile != null && tile.IsWalkable())
                    {
                        tiles.Add(tile);
                        tile.AbilityEffect = this;
                    }
                }
            }

            tileTimer.ResetTimerAndRun();
            base.ActivateFinish(direction);
        }



        public override void ResetOnDeath()
        {
            base.ResetOnDeath();

            ClearTiles();
            tileTimer.State = TimerState.Stopped;
        }

        public void ClearTiles()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].AbilityEffect == this)
                {
                    tiles[i].AbilityEffect = null;
                }
            }
            tiles.Clear();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (tileTimer.Update(gameTime))
            {
                ClearTiles();
            }

            if (damageEnemiesTimer.Update(gameTime))
            {
                alreadyDamagedEnemies.Clear();
            }
        }


        public override void Select()
        {
            // set the players active ability to this one
            player.SetActiveAbility(this);

            base.Select();
        }



        public override string Description
        {
            get
            {
                int offset = 0;
                if (level != 0)
                    offset = level - 1;
                return "Freezes an area on the ground causing enemies that walk on it to be slowed and dealing " + damage[offset].ToString() + " ice damage per second.";
            }
        }

        public override string NextDescription
        {
            get
            {
                if (level == MaxLevel)
                    return String.Empty;

                return "Increase the duration of the slow effect and damage dealt to " + damage[level - 1].ToString() + ".";
            }
        }


        public override Color GetAbilityTint()
        {
            return Color.MediumBlue;
        }


        public override void TileAbilityEfffect(CharacterSprite sprite)
        {
            base.TileAbilityEfffect(sprite);

            if (sprite is EnemySprite)
            {
                sprite.SlowCharacter(slowTime[level - 1], .5f, player);

                EnemySprite enemy = (EnemySprite)sprite;
                if (!alreadyDamagedEnemies.Contains(enemy) && !enemy.IsDead)
                {
                    alreadyDamagedEnemies.Add(enemy);
                    enemy.Damage(damage[level - 1] + player.SpellDamage / 10, DamageType.Ice, player);
                    ParticleSystem.AddParticles(enemy.CenteredPosition, ParticleType.ExplosionWhite, sizeScale: 1f, lifetimeScale: 1f, numParticlesScale: 1.0f, color:Color.MediumBlue);
                }
            }
        }
    }
}
