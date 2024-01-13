using fNbt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NBTViewer
{
    abstract class NbtTreeContainer
    {
        private static object dummyNode = null;

        public static void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                if (item.Tag != null)
                {
                    NbtTag last = item.Tag as NbtTag;
                    if (last is ICollection<NbtTag> nbtCol)
                    {
                        foreach (NbtTag tag in nbtCol)
                        {
                            if (tag is ICollection<NbtTag>)
                            {
                                item.Items.Add(collectionItem(tag));
                            }
                            else
                            {
                                item.Items.Add(valueItem(tag));
                            }
                        }
                    }
                    else
                    {
                        item.Items.Add(valueItem(last));
                    }
                }
            }
        }

        public static TreeViewItem valueItem(NbtTag tag)
        {
            TreeViewItem subitem = new TreeViewItem();
            string format = String.Format("{0}(\"{1}\"): ", NbtTag.GetCanonicalTagName(tag.TagType), tag.name);
            string entriesString = "";

            if (tag is NbtIntArray)
            {
                entriesString = "int[" + tag.IntArrayValue.Length + "]";
            }
            else if (tag is NbtByteArray)
            {
                entriesString = "byte[" + tag.ByteArrayValue.Length + "]";
            }
            else if (tag is NbtLongArray)
            {
                entriesString = "long[" + tag.LongArrayValue.Length + "]";
            }
            else
            {
                entriesString = tag.StringValue;
            }

            setInlineHeader(subitem, format, entriesString);

            return subitem;
        }

        public static TreeViewItem collectionItem(NbtTag tag)
        {
            TreeViewItem subitem = new TreeViewItem();

            int elemsCount = ((ICollection<NbtTag>)tag).Count;
            setInlineHeader(subitem, String.Format("{0}(\"{1}\"): ", NbtTag.GetCanonicalTagName(tag.TagType), tag.name),
                elemsCount + " " + (elemsCount == 1 ? "Entry" : "Entries"));

            if (elemsCount > 0)
            {
                subitem.Tag = tag;
                subitem.Items.Add(dummyNode);
                subitem.Expanded += new RoutedEventHandler(folder_Expanded);
            }

            return subitem;
        }

        private static void setInlineHeader(TreeViewItem item, string key, string val)
        {
            var textblock = new TextBlock();
            textblock.Inlines.Add(new Run { Text = key });
            textblock.Inlines.Add(new Run { Text = val, FontWeight = FontWeights.Bold });
            item.Header = textblock;
        }

        public static NbtCompound loadNbtFile(string path)
        {
            NbtFile nbt = new NbtFile();
            nbt.LoadFromFile(path);
            return nbt.RootTag;
        }

        private const int SECTOR_BYTES = 4096;

        public static NbtCompound loadRegionFile(string path)
        {
            using (var file = File.OpenRead(path))
            {
                var chunksToParse = new List<uint>();

                do
                {
                    uint b1 = (uint)file.ReadByte();
                    uint b2 = (uint)file.ReadByte();
                    uint b3 = (uint)file.ReadByte();
                    uint offset = (b1 << 16 | b2 << 8 | b3) * SECTOR_BYTES;

                    int length = file.ReadByte() * SECTOR_BYTES;

                    if (offset != 0 && length != 0)
                    {
                        chunksToParse.Add(offset);
                    }
                } while (file.Position != SECTOR_BYTES);

                NbtCompound chunksRoot = new NbtCompound();

                foreach (var chunkOffset in chunksToParse)
                {
                    file.Seek(chunkOffset + 5, SeekOrigin.Begin);

                    NbtFile nbt = new NbtFile();
                    nbt.LoadFromStream(file, NbtCompression.AutoDetect);
                    NbtCompound chunk = nbt.RootTag;
                    NbtCompound dataTag = chunk["Level"] as NbtCompound;

                    if (dataTag != null)
                    {
                        chunk.name = dataTag["xPos"].IntValue + ", " + dataTag["zPos"].IntValue;
                        chunksRoot.Add(chunk);
                    }
                }

                return chunksRoot;
            }
        }
    }

    class FileTab
    {
        public string header { get; set; }
        public TreeViewItem treeRoot { get; set; }

        public FileTab()
        {
            header = "File";
            treeRoot = new TreeViewItem();
            treeRoot.Items.Add("No NBT loaded");
        }

        public FileTab(TreeViewItem nbtTree, string filename)
        {
            treeRoot = nbtTree;
            header = filename;
        }
    }
}
