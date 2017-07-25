﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace ACSE
{
    public static class Utility
    {
        public static void Scan_For_NL_Int32()
        {
            if (NewMainForm.Save_File != null && (NewMainForm.Save_File.Save_Type == SaveType.New_Leaf || NewMainForm.Save_File.Save_Type == SaveType.Welcome_Amiibo))
            {
                using (StreamWriter Int32_Stream = File.CreateText(NewMainForm.Assembly_Location + "\\" +
                    (NewMainForm.Save_File.Save_Type == SaveType.Welcome_Amiibo ? "WA_" : "") + "NL_Int32_Database.txt"))
                    for (int i = 0; i < NewMainForm.Save_File.Working_Save_Data.Length - 4; i += 4)
                    {
                        NL_Int32 Possible_NL_Int32 = new NL_Int32(NewMainForm.Save_File.ReadUInt32(i), NewMainForm.Save_File.ReadUInt32(i + 4));
                        if (Possible_NL_Int32.Valid)
                            Int32_Stream.WriteLine(string.Format("Found Valid NL_Int32 at offset 0x{0} | Value: {1}", i.ToString("X"), Possible_NL_Int32.Value));
                    }
            }
        }

        public static Image Set_Image_Color(Image Grayscale_Image, ColorMatrix Transform_Matrix)
        {
            using (ImageAttributes Attributes = new ImageAttributes())
            {
                Attributes.SetColorMatrix(Transform_Matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                Bitmap Transformed_Image = new Bitmap(Grayscale_Image.Width, Grayscale_Image.Height);
                using (Graphics G = Graphics.FromImage(Transformed_Image))
                {
                    G.DrawImage(Grayscale_Image, 0, 0);
                    G.DrawImage(Transformed_Image, new Rectangle(0, 0, Grayscale_Image.Size.Width, Grayscale_Image.Size.Height),
                        0, 0, Grayscale_Image.Size.Width, Grayscale_Image.Size.Height, GraphicsUnit.Pixel, Attributes);
                    return Transformed_Image;
                }
            }
        }

        public static void Increment_Town_ID(Save Save_File)
        {
            int Total_IDs = 0;
            ushort Town_ID = Save_File.ReadUInt16(Save_File.Save_Data_Start_Offset + 8, true);
            for (int i = 0x26040; i < 0x4C040; i += 2)
            {
                ushort Value = Save_File.ReadUInt16(i, true);
                if (Value == Town_ID)
                {
                    Total_IDs++;
                    Save_File.Write(i, (ushort)(Value + 1), true);
                }
            }
            System.Windows.Forms.MessageBox.Show("Total IDs Replaced: " + Total_IDs);
            Save_File.Flush();
        }

        public static byte[] Find_Villager_House(ushort Villager_ID) // TODO: Apply to WW
        {
            if (NewMainForm.Save_File != null)
            {
                ushort Villager_House_ID = (ushort)(0x5000 + (Villager_ID & 0xFF));
                foreach (Normal_Acre Acre in NewMainForm.Town_Acres)
                {
                    WorldItem Villager_House = Acre.Acre_Items.FirstOrDefault(o => o.ItemID == Villager_House_ID);
                    if (Villager_House != null)
                    {
                        return new byte[4] { (byte)(Acre.Index % 7), (byte)(Acre.Index / 7), (byte)(Villager_House.Location.X), (byte)(Villager_House.Location.Y + 1) };
                    }
                }
            }
            return new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };
        }
    }
}
