using System.Collections;
using Lachesis.Core;
using UnityEngine;
using ProcedureOwner = FSM<Lachesis.Core.ProcedureManager>;

namespace Lachesis.GamePlay
{
    public class ProcedureBattle : ProcedureBase
    {
        //private bool changeScene;
        private ProcedureOwner procedureOwner;

        protected internal override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected internal override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            this.procedureOwner = procedureOwner;
            //changeScene = false;
            Debug.Log("进入主战斗流程");
            GameEntry.instance.StartCoroutine(LoadResourceAsync("Prefabs/CarSport"));
        }

        protected internal override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        }

        protected internal override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected internal override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }

        private IEnumerator LoadResourceAsync(string resourcePath)
        {
            var request = Resources.LoadAsync<GameObject>(resourcePath);
            yield return request;

            var prefab = request.asset as GameObject;
            if (prefab != null)
            {
                var go = CreateCar(prefab, Vector3.zero, Quaternion.identity);
                CreateCar(prefab, GameEntry.instance.globalConfig.Player1StarPosition + new Vector3(2, 0, 0), Quaternion.identity);
                CreateCar(prefab, GameEntry.instance.globalConfig.Player1StarPosition + new Vector3(-2, 0, 0), Quaternion.identity);
                var player = go.AddComponent<Player>();
                go.name = "playerCar";
                player.carController = go.GetComponent<CarController>();
                if (player.carController == null) Debug.LogError("未找到CarController");
            }
            else
            {
                Debug.LogError("Prefab not found in Resources folder");
            }
        }

        private GameObject CreateCar(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            var go = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            go.transform.position = pos;
            go.transform.rotation = rot;
            return go;
        }
    }
}