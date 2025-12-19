using UnityEngine;
using UnityEditor;
using System.IO;

namespace BloodSystem.Editor
{
    /// <summary>
    /// 피 스플래터 텍스처를 프로시저럴 방식으로 생성하는 에디터 도구
    /// </summary>
    public class SplatTextureGenerator : EditorWindow
    {
        private int textureSize = 256;
        private int splatCount = 4;
        private string savePath = "Assets/BloodSystem/Textures/Splatters/";

        [MenuItem("Tools/Blood System/Generate Splatter Textures")]
        public static void ShowWindow()
        {
            GetWindow<SplatTextureGenerator>("Splatter Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Splatter Texture Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            textureSize = EditorGUILayout.IntSlider("Texture Size", textureSize, 64, 512);
            splatCount = EditorGUILayout.IntSlider("Splat Count", splatCount, 1, 10);
            savePath = EditorGUILayout.TextField("Save Path", savePath);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Splatter Textures", GUILayout.Height(40)))
            {
                GenerateSplatters();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "이 도구는 프로시저럴 방식으로 피 스플래터 텍스처를 생성합니다.\n" +
                "생성된 텍스처는 수정 가능하며, 직접 그린 텍스처로 교체할 수도 있습니다.",
                MessageType.Info
            );
        }

        private void GenerateSplatters()
        {
            // 경로 확인 및 생성
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            for (int i = 0; i < splatCount; i++)
            {
                Texture2D splatter = GenerateSingleSplatter(textureSize, i);
                SaveTexture(splatter, $"Splatter_{i + 1:00}.png");
                DestroyImmediate(splatter);
            }

            AssetDatabase.Refresh();
            Debug.Log($"스플래터 텍스처 {splatCount}개가 생성되었습니다: {savePath}");
        }

        private Texture2D GenerateSingleSplatter(int size, int seed)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Random.InitState(seed * 12345);

            // 스플래터 파라미터
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxRadius = size * 0.4f;
            int branchCount = Random.Range(8, 16);

            Color[] pixels = new Color[size * size];

            // 초기화 (투명)
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            // 메인 스플래터 (원형)
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);

                    if (dist < maxRadius)
                    {
                        // 중앙에서 가장자리로 갈수록 투명해짐
                        float normalizedDist = dist / maxRadius;
                        float alpha = 1f - normalizedDist;
                        alpha = Mathf.Pow(alpha, 1.5f); // 부드러운 감쇠

                        // Perlin noise로 자연스러운 엣지
                        float noise = Mathf.PerlinNoise(x * 0.1f + seed, y * 0.1f + seed);
                        alpha *= Mathf.Lerp(0.7f, 1f, noise);

                        int index = y * size + x;
                        pixels[index] = new Color(alpha, alpha, alpha, 1f);
                    }
                }
            }

            // 가지/튀김 추가
            for (int i = 0; i < branchCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float branchLength = Random.Range(maxRadius * 0.3f, maxRadius * 0.8f);
                float branchWidth = Random.Range(2f, 8f);

                DrawBranch(pixels, size, center, direction, branchLength, branchWidth);
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        private void DrawBranch(Color[] pixels, int size, Vector2 start, Vector2 direction, float length, float width)
        {
            int steps = Mathf.CeilToInt(length);

            for (int i = 0; i < steps; i++)
            {
                float t = i / (float)steps;
                Vector2 pos = start + direction * (t * length);

                // 끝으로 갈수록 가늘어짐
                float currentWidth = width * (1f - t);
                float alpha = 1f - t;

                // 원형 브러시로 그리기
                for (int dy = -(int)currentWidth; dy <= (int)currentWidth; dy++)
                {
                    for (int dx = -(int)currentWidth; dx <= (int)currentWidth; dx++)
                    {
                        int x = Mathf.RoundToInt(pos.x) + dx;
                        int y = Mathf.RoundToInt(pos.y) + dy;

                        if (x >= 0 && x < size && y >= 0 && y < size)
                        {
                            float dist = Mathf.Sqrt(dx * dx + dy * dy);
                            if (dist <= currentWidth)
                            {
                                float brushAlpha = alpha * (1f - dist / currentWidth);
                                int index = y * size + x;

                                // Max 블렌딩
                                float currentAlpha = pixels[index].r;
                                float newAlpha = Mathf.Max(currentAlpha, brushAlpha);
                                pixels[index] = new Color(newAlpha, newAlpha, newAlpha, 1f);
                            }
                        }
                    }
                }
            }
        }

        private void SaveTexture(Texture2D texture, string fileName)
        {
            byte[] bytes = texture.EncodeToPNG();
            string fullPath = Path.Combine(savePath, fileName);
            File.WriteAllBytes(fullPath, bytes);

            // Import 설정
            AssetDatabase.ImportAsset(fullPath);
            TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false; // Linear
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = false;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}
