using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Lachesis.GamePlay
{
    public class CreateAtlasFromFolderEditor
    {
        [MenuItem("Assets/从文件夹创建Sprite Atlas")]
        public static void CreateSpriteAtlasFromFolder()
        {
            // 获取选定的文件夹路径
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("请选择一个文件夹");
                return;
            }

            // 使用文件夹名称作为Sprite Atlas的名称
            string folderName = Path.GetFileName(folderPath);
            string atlasPath = $"{folderPath}/{folderName}.spriteatlas";

            // 检查是否已经存在一个具有相同名称的Sprite Atlas
            if (File.Exists(atlasPath))
            {
                // 弹出提示对话框
                if (!EditorUtility.DisplayDialog("提示",
                        $"A Sprite Atlas named '{folderName}' already exists in the folder. Do you want to overwrite it?",
                        "Yes", "No"))
                {
                    // 用户选择了取消，不覆盖现有的Sprite Atlas
                    return;
                }

                // 删除现有的Sprite Atlas
                AssetDatabase.DeleteAsset(atlasPath);
            }

            // 创建一个新的Sprite Atlas
            SpriteAtlas spriteAtlas = new SpriteAtlas();
            AssetDatabase.CreateAsset(spriteAtlas, atlasPath);

            // 获取文件夹中的所有Sprite
            string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
            Sprite[] sprites = spriteGuids.Select(guid => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();

            // 将Sprite添加到Sprite Atlas
            spriteAtlas.Add(sprites);

            // 保存更改
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Sprite Atlas created at: {atlasPath}");
        }
        
    }
}

