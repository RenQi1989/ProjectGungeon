using UnityEngine;
using QFramework;
using System.Collections.Generic;

namespace QFramework.ProjectGungeon
{
    public partial class MP5 : GunManager
    {
        public override PlayerBullet bulletPrefab => PlayerBullet;
        public override AudioSource audioPlayer => SelfAudioSource;
        private float currentSecond = 0f; // 连续射击
        private ShootDuration shootDuration = new ShootDuration(shootDuration: 0f, chargeTime: 0f);
        private GunClip gunClip = new GunClip(clipCapacity: 40, currentClipCapacity: 40, bulletBackpack: 80);
        private ShootFire shootFire = new ShootFire();
        public override bool IsReloading => gunClip.isReloading;
        public AudioSource emptyClipAudioPlayer;
        public AudioClip emptyClipSound;

        // 更新武器装备状态
        public override void IsEquipped()
        {
            if (gunClip.CurrentClipCapacity == 0) // 主角当前弹夹子弹数为0时，显示提示文字
            {
                Player.Default.DisplayTextOnPlayer("No Bullet!", 2);
            }
            gunClip.UpdateUI();
        }

        private void Update()
        {
            gunClip.BulletReload(reloadSound); // 换弹夹
        }

        // MP5的射击方式：鼠标按下后可以连续开枪
        // 射击逻辑
        public void Shoot(Vector2 shootDirection)
        {
            var playerBullet = Instantiate(bulletPrefab);

            // 子弹发射的位置就是子弹模板的位置（枪口处）
            playerBullet.transform.position = bulletPrefab.transform.position;
            playerBullet.velocity = shootDirection.normalized * 20;
            playerBullet.gunDamage = 1;
            playerBullet.gameObject.SetActive(true); // 把在Inspector失活的子弹重新激活

            // 播放射击音效																				 
            audioPlayer.clip = shootSounds[0];
            audioPlayer.loop = true; // 循环
            audioPlayer.Play();

            // 子弹消耗
            gunClip.UseBullet();

            // 更新武器装备状态
            IsEquipped();

            // 开枪火花
            shootFire.ShowShootFire(bulletPrefab.Position2D(), shootDirection);
        }

        // 开始射击
        public override void ShootMouseDown(Vector2 shootDirection)
        {
            if (shootDuration.CanShoot && gunClip.CanShoot)
            {
                Shoot(shootDirection);
            }

            if (gunClip.CurrentClipCapacity == 0) // 当前弹夹子弹数为0，玩家抬起鼠标时，播放空弹夹音
            {
                emptyClipAudioPlayer.PlayOneShot(emptyClipSound);
            }
        }

        public override void Shooting(Vector2 shootDirection)
        {
            if (shootDuration.CanShoot && gunClip.CanShoot && currentSecond >= 0.1f && !IsReloading)
            {
                Shoot(shootDirection);
                currentSecond = 0f;
            }
            else if (!gunClip.CanShoot)
            {
                audioPlayer.Stop();
            }

            currentSecond += Time.deltaTime;
        }

        // 停止射击
        public override void ShootMouseUp(Vector2 shootingDirection)
        {
            audioPlayer.Stop();
        }
    }
}
