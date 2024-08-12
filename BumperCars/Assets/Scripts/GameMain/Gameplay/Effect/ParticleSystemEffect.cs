using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lachesis.GamePlay
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemEffect : Entity
    {
        private ParticleSystem m_particleSys;

        public override void OnInit(object userData = null)
        {
            base.OnInit(userData);
            m_particleSys = GetComponent<ParticleSystem>();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if(m_particleSys.isStopped) //如果停止了，则回收自己
            {
                GameEntry.EntityManager.ReturnEntity(entityEnum, this);
            }
        }

        public override void OnReCreateFromPool(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnReCreateFromPool(pos, rot, userData);
            m_particleSys.Play();
        }
        public override void OnReCreateFromPool( object userData = null)
        {
            base.OnReCreateFromPool(userData);
            transform.localPosition = Vector3.zero;
            if(userData is Vector3 offset)
            {
                transform.localPosition+=offset;
            }
            m_particleSys.Play();
        }

        public override void OnReturnToPool(bool isShutDown = false)
        {
            base.OnReturnToPool(isShutDown);
            if(!isShutDown) m_particleSys.Stop();
        }
    }
}

