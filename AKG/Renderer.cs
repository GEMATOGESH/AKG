﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AKG
{
    public class Renderer
    {
        private Image image;

        //private Vector3 normal, lightDirection;

        private Vector3 ambientColor = new Vector3 ( 9, 56, 97 );
        private Vector3 diffuseColor = new Vector3 ( 87, 171, 105 );
        private Vector3 specularColor = new Vector3 ( 212, 21, 21 ); 

        private float ambientFactor = 1.0f;
        private float diffuseFactor = 0.8f;
        private float specularFactor = 0.3f;

        private float glossFactor = 0.3f;

        private Vector3 objectColor = new(255, 255, 255);
        private Vector3 lightColor = new(0.5f, 0.5f, 0f);

        private Dictionary<Vector3, List<Vector3>> triangleNormals = new();
        private Dictionary<Vector3, Vector3> vertexNormals = new();

        public Renderer(Image image)
        {
            this.image = image;
        }

        private int[] AmbientLightning()
        {
            int[] values = new int[3];

            values[0] = (int)(ambientColor.X * ambientFactor);
            values[1] = (int)(ambientColor.Y * ambientFactor);
            values[2] = (int)(ambientColor.Z * ambientFactor);            

            return values;
        }

        private int[] DiffuseLightning(float intensity)
        {
            int[] values = new int[3];

            values[0] = (int)(diffuseFactor * intensity * diffuseColor.X);
            values[1] = (int)(diffuseFactor * intensity * diffuseColor.Y);
            values[2] = (int)(diffuseFactor * intensity * diffuseColor.Z);
            
            return values;
        }

        private int[] SpecularLightning(Vector3 View, Vector3 lightDirection, Vector3 normal)
        {
            Vector3 reflection = Vector3.Normalize(Vector3.Reflect(lightDirection, normal));
            float RV = Math.Max(Vector3.Dot(reflection, View), 0);

            int[] values = new int[3];
            double temp = Math.Pow(RV, glossFactor);

            values[0] = (int)(specularFactor * temp * specularColor.X);
            values[1] = (int)(specularFactor * temp * specularColor.Y);
            values[2] = (int)(specularFactor * temp * specularColor.Z);

            return values;
        }

        private unsafe void DrawPixel(WriteableBitmap bitmap, int x, int y, int[] ambientValues, int[] diffuseValues, int[] specularValues)
        {
            if (x > 0 && y > 0 && x < VectorTransformation.width && y < VectorTransformation.height)
            {
                byte* temp = (byte*)bitmap.BackBuffer + y * bitmap.BackBufferStride + x * bitmap.Format.BitsPerPixel / 8;

                temp[3] = 255;
                temp[2] = (byte)Math.Min(ambientValues[0] + diffuseValues[0] + specularValues[0], 255);
                temp[1] = (byte)Math.Min(ambientValues[1] + diffuseValues[1] + specularValues[1], 255);
                temp[0] = (byte)Math.Min(ambientValues[2] + diffuseValues[2] + specularValues[2], 255);
            }
        }

        public void DrawModel()
        {
            if (VectorTransformation.width == 0)
            {
                VectorTransformation.width = 1;
            }
            if (VectorTransformation.height == 0)
            {
                VectorTransformation.height = 1;
            }

            WriteableBitmap bitmap = new((int)VectorTransformation.width, (int)VectorTransformation.height, 96, 96, PixelFormats.Bgra32, null);
            bitmap.Lock();

            Rasterization(bitmap);

            bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            bitmap.Unlock();

            image.Source = bitmap;
        }

        private void FindNormals()
        {
            triangleNormals.Clear();
            vertexNormals.Clear();

            foreach (int[] vector in Model.listF)
            {
                for (int j = 3; j < vector.Length - 3; j += 3)
                {
                    //Vector4[] screenTriangle = { Model.screenVertices[vector[0] - 1], Model.screenVertices[vector[j] - 1], Model.screenVertices[vector[j + 3] - 1] };
                    Vector3[] worldTriangle = { Model.worldVertices[vector[0] - 1], Model.worldVertices[vector[j] - 1], Model.worldVertices[vector[j + 3] - 1] };

                    //Отбраковка невидимых поверхностей.
                    /*Vector4 edge1 = screenTriangle[2] - screenTriangle[0];
                    Vector4 edge2 = screenTriangle[1] - screenTriangle[0];

                    if (edge1.X * edge2.Y - edge1.Y * edge2.X <= 0)
                    {
                        continue;
                    }*/

                    Vector3 worldEdge1 = worldTriangle[1] - worldTriangle[0];
                    Vector3 worldEdge2 = worldTriangle[2] - worldTriangle[0];

                    Vector3 normal = Vector3.Cross(worldEdge1, worldEdge2);
                    normal = Vector3.Normalize(normal);

                    foreach (Vector3 vertex in worldTriangle)
                    {
                        if (triangleNormals.ContainsKey(vertex))
                        {
                            triangleNormals[vertex].Add(normal);
                        }
                        else
                        {
                            triangleNormals.Add(vertex, new List<Vector3>() { normal });
                        }
                    }
                }
            }

            foreach(var item in triangleNormals)
            {
                Vector3 temp = Vector3.Zero;

                for(int i = 0; i < item.Value.Count; i++)
                {
                    temp += item.Value[i];
                }

                vertexNormals.Add(item.Key, temp / item.Value.Count);
            }
        }

        private void Rasterization(WriteableBitmap bitmap)
        {
            float?[,] z_buff = new float?[bitmap.PixelHeight, bitmap.PixelWidth];

            FindNormals();

            foreach (int[] vector in Model.listF)
            {
                // Итерация по треугольникам в полигоне.
                for (int j = 3; j < vector.Length - 3; j += 3)
                {
                    // Формирование треугольников в экранных и мировых координатах.
                    Vector4[] screenTriangle = { Model.screenVertices[vector[0] - 1], Model.screenVertices[vector[j] - 1], Model.screenVertices[vector[j + 3] - 1] };
                    Vector3[] worldTriangle = { Model.worldVertices[vector[0] - 1], Model.worldVertices[vector[j] - 1], Model.worldVertices[vector[j + 3] - 1] };

                    // Отбраковка невидимых поверхностей.
                    Vector4 edge1 = screenTriangle[2] - screenTriangle[0];
                    Vector4 edge2 = screenTriangle[1] - screenTriangle[0];
                    if (edge1.X * edge2.Y - edge1.Y * edge2.X <= 0)
                    {
                        continue;
                    }

                    // Сортировка вершин треугольников в порядке "вверх-лево-право(низ)".
                    if (screenTriangle[0].Y > screenTriangle[1].Y)
                    {
                        (screenTriangle[0], screenTriangle[1]) = (screenTriangle[1], screenTriangle[0]);
                        (worldTriangle[0], worldTriangle[1]) = (worldTriangle[1], worldTriangle[0]);
                    }
                    if (screenTriangle[0].Y > screenTriangle[2].Y)
                    {
                        (screenTriangle[0], screenTriangle[2]) = (screenTriangle[2], screenTriangle[0]);
                        (worldTriangle[0], worldTriangle[2]) = (worldTriangle[2], worldTriangle[0]);
                    }
                    if (screenTriangle[1].Y > screenTriangle[2].Y)
                    {
                        (screenTriangle[1], screenTriangle[2]) = (screenTriangle[2], screenTriangle[1]);
                        (worldTriangle[1], worldTriangle[2]) = (worldTriangle[2], worldTriangle[1]);
                    }

                    // Поиск нормали по вершинам.
                    Vector3 vertexNormal0 = vertexNormals[new Vector3(worldTriangle[0].X, worldTriangle[0].Y, worldTriangle[0].Z)];
                    Vector3 vertexNormal1 = vertexNormals[new Vector3(worldTriangle[1].X, worldTriangle[1].Y, worldTriangle[1].Z)];
                    Vector3 vertexNormal2 = vertexNormals[new Vector3(worldTriangle[2].X, worldTriangle[2].Y, worldTriangle[2].Z)];

                    // Нахождение коэффицентов в экранных и мировых координатах, коэффицента для нормалей.
                    Vector4 screenKoeff01       = (screenTriangle[1] - screenTriangle[0]) / (screenTriangle[1].Y - screenTriangle[0].Y);
                    Vector3 worldKoeff01        = (worldTriangle[1] - worldTriangle[0])   / (screenTriangle[1].Y - screenTriangle[0].Y);
                    Vector3 vertexNormalKoeff01 = (vertexNormal1 - vertexNormal0)         / (screenTriangle[1].Y - screenTriangle[0].Y);

                    Vector4 screenKoeff02       = (screenTriangle[2] - screenTriangle[0]) / (screenTriangle[2].Y - screenTriangle[0].Y);
                    Vector3 worldKoeff02        = (worldTriangle[2] - worldTriangle[0])   / (screenTriangle[2].Y - screenTriangle[0].Y);
                    Vector3 vertexNormalKoeff02 = (vertexNormal2 - vertexNormal0)         / (screenTriangle[2].Y - screenTriangle[0].Y);

                    Vector4 screenKoeff03       = (screenTriangle[2] - screenTriangle[1]) / (screenTriangle[2].Y - screenTriangle[1].Y);
                    Vector3 worldKoeff03        = (worldTriangle[2] - worldTriangle[1])   / (screenTriangle[2].Y - screenTriangle[1].Y);
                    Vector3 vertexNormalKoeff03 = (vertexNormal2 - vertexNormal1)         / (screenTriangle[2].Y - screenTriangle[1].Y);

                    // Нахождение минимального и максимального Y для треугольника на экране и последующей итерации.
                    int minY = Math.Max((int)MathF.Ceiling(screenTriangle[0].Y), 0);
                    int maxY = Math.Min((int)MathF.Ceiling(screenTriangle[2].Y), bitmap.PixelHeight);

                    for (int y = minY; y < maxY; y++)
                    {
                        // Нахождение левого и правого значения Y.
                        Vector4 screenA = y < screenTriangle[1].Y ? screenTriangle[0] + (y - screenTriangle[0].Y) * screenKoeff01 :
                                                                    screenTriangle[1] + (y - screenTriangle[1].Y) * screenKoeff03; 
                        Vector4 screenB = screenTriangle[0] + (y - screenTriangle[0].Y) * screenKoeff02;

                        Vector3 worldA = y < screenTriangle[1].Y ? worldTriangle[0] + (y - screenTriangle[0].Y) * worldKoeff01 :
                                                                   worldTriangle[1] + (y - screenTriangle[1].Y) * worldKoeff03;
                        Vector3 worldB = worldTriangle[0] + (y - screenTriangle[0].Y) * worldKoeff02;

                        // Нахождение нормали для левого и правого Y.
                        Vector3 normalA = y < screenTriangle[1].Y ? vertexNormal0 + (y - screenTriangle[0].Y) * vertexNormalKoeff01 :
                                                                    vertexNormal1 + (y - screenTriangle[1].Y) * vertexNormalKoeff03;
                        Vector3 normalB = vertexNormal0 + (y - screenTriangle[0].Y) * vertexNormalKoeff02;

                        // Сортировка значений в порядке "лево-право".
                        if (screenA.X > screenB.X)
                        {
                            (screenA, screenB) = (screenB, screenA);
                            (worldA , worldB)  = (worldB , worldA);
                            (normalA, normalB) = (normalB, normalA);
                        }

                        // Нахождение минимального и максимального X для треугольника на экране и последующей итерации.
                        int minX = Math.Max((int)MathF.Ceiling(screenA.X), 0);
                        int maxX = Math.Min((int)MathF.Ceiling(screenB.X), bitmap.PixelWidth);

                        // Нахождение коэффицентов изменения X в экранных и мировых координатах, коэффицента изменения нормали.
                        Vector4 screenKoeff = (screenB - screenA) / (screenB.X - screenA.X);
                        Vector3 worldKoeff  = (worldB - worldA)   / (screenB.X - screenA.X);
                        Vector3 normalKoeff = (normalB - normalA) / (screenB.X - screenA.X);

                        // Сканирующая линия.
                        for (int x = minX; x < maxX; x++)
                        {
                            // Нахождение координат точки в экранных и мировых координатах.
                            Vector4 pScreen = screenA + (x - screenA.X) * screenKoeff;
                            Vector3 pWorld = worldA + (x - screenA.X) * worldKoeff;

                            // Z-буффер.
                            if (!(pScreen.Z > z_buff[y, x]))
                            {
                                z_buff[y, x] = pScreen.Z;

                                // Нахождение обратного вектора направления света.
                                Vector3 lightDirection = Vector3.Normalize(VectorTransformation.light - pWorld);

                                // Нахождение нормали для точки.
                                Vector3 normal = normalA + (x - screenA.X) * normalKoeff;
                                normal = Vector3.Normalize(normal);

                                // Нахождение дистанции до источника света.
                                float distance = lightDirection.LengthSquared();
                                // Затенение объекта в зависимости от дистанции света до модели.
                                float attenuation = 1 / Math.Max(distance, 0.01f);
                                // Получение затененности каждой точки.
                                float intensity = Math.Max(Vector3.Dot(normal, lightDirection), 0);

                                // Освещение Фонга.
                                int[] ambientValues = AmbientLightning();
                                int[] diffuseValues = DiffuseLightning(intensity * attenuation);
                                int[] specularValues = SpecularLightning(pWorld - VectorTransformation.eye, lightDirection, normal);

                                // Отрисовка.
                                DrawPixel(bitmap, x, y, ambientValues, diffuseValues, specularValues);
                            }
                        }
                    }
                }
            }
        }
    }
}
