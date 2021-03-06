﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using ZeldaFullEditor.Properties;
using Microsoft.VisualBasic;
using System.IO.Compression;
using static ZeldaFullEditor.zscreamForm;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using WeifenLuo.WinFormsUI.Docking;

namespace ZeldaFullEditor
{
    public class SceneUW : Scene
    {

        public SceneUW(zscreamForm f)
        {
            graphics = Graphics.FromImage(scene_bitmap);
            
            this.MouseDown += new MouseEventHandler(onMouseDown);
            this.MouseUp += new MouseEventHandler(onMouseUp);
            this.MouseMove += new MouseEventHandler(onMouseMove);
            this.MouseDoubleClick += new MouseEventHandler(onMouseDoubleClick);
            
            mainForm = f;
        }

        public override void Clear()
        {
            graphics.Clear(this.BackColor);
        }

        public void ChangeRoom(Room room)
        {
            this.room = room;
        }

        private void onMouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();
            mainForm.activeScene = this;
            mainForm.mapPropertyGrid.SelectedObject = room;
            room.reloadGfx();
            need_refresh = true;
            if (e.Button == MouseButtons.Left)
            {

                if (selectedDragObject != null)
                {
                    
                    room.selectedObject.Clear();
                    Room_Object ro = room.addObject(selectedDragObject.id, (byte)0, (byte)0, 0, (byte)selectedMode);
                    if (ro != null)
                    {
                        ro.setRoom(room);
                        ro.get_scroll_x();
                        ro.get_scroll_y();
                        if (ro.special_zero_size != 0)
                        {
                            ro.size = 1;
                        }


                        room.tilesObjects.Add(ro);
                        room.selectedObject.Add(ro);
                        dragx = 0;
                        dragy = 0;

                    }
                    room.has_changed = true;
                    mouse_down = true;
                    need_refresh = true;
                    room.reloadGfx(false);
                    mainForm.objectsListbox.ClearSelected();
                    selectedDragObject = null;
                }

                if (selectedDragSprite != null)
                {
                    room.selectedObject.Clear();
                    Sprite spr = new Sprite(room, (byte)selectedDragSprite.id, 0, 0, selectedDragSprite.Name, 0, 0, 0);
                    room.sprites.Add(spr);
                    room.selectedObject.Add(spr);
                    mouse_down = true;
                    dragx = 0;
                    dragy = 0;
                    room.has_changed = true;
                    need_refresh = true;
                    //reloadgfx ?
                    mainForm.spritesListbox.ClearSelected();
                    selectedDragSprite = null;
                }






                if (mouse_down == false)
                {
                    doorArray = new Rectangle[48];
                    if (resizing != ObjectResize.None)
                    {
                        selection_resize = true;
                        mouse_down = true;
                        dragx = ((e.X) / 8);
                        dragy = ((e.Y) / 8);
                        (room.selectedObject[0] as Room_Object).oldSize = (room.selectedObject[0] as Room_Object).size;
                        (room.selectedObject[0] as Room_Object).savedSize = (room.selectedObject[0] as Room_Object).size;
                        return;
                    }
                    found = false;
                    if (selectedMode == ObjectMode.Spritemode)
                    {
                        dragx = ((e.X) / 16);
                        dragy = ((e.Y) / 16);
                        if (room.selectedObject.Count == 1)
                        {
                            room.selectedObject.Clear();
                        }
                        foreach (Sprite spr in room.sprites)
                        {
                            if (isMouseCollidingWith(spr, e))
                            {
                                if (spr.selected == false)
                                {
                                    room.selectedObject.Add(spr);
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (found == false) //we didnt find any sprites to click on so just clear the selection
                        {
                            room.selectedObject.Clear();
                        }
                    }
                    else if (selectedMode == ObjectMode.Itemmode)
                    {
                        dragx = ((e.X) / 8);
                        dragy = ((e.Y) / 8);
                        if (room.selectedObject.Count == 1)
                        {
                            foreach (Object o in room.pot_items)
                            {
                                //(o as PotItem).selected = false;
                            }
                            room.selectedObject.Clear();
                        }
                        foreach (PotItem item in room.pot_items)
                        {
                            if (isMouseCollidingWith(item, e))
                            {
                                if (item.selected == false)
                                {
                                    room.selectedObject.Add(item);
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (found == false) //we didnt find any items to click on so just clear the selection
                        {
                            room.selectedObject.Clear();
                        }
                    }
                    else if ((byte)selectedMode >= 0 && (byte)selectedMode <= 3)
                    {

                        dragx = ((e.X) / 8);
                        dragy = ((e.Y) / 8);
                        bool already_in = false;
                        Room_Object objectFound = null;
                        found = false;
                        for (int i = room.tilesObjects.Count - 1; i >= 0; i--)
                        {
                            Room_Object obj = room.tilesObjects[i];
                            if (isMouseCollidingWith(obj, e))
                            {
                                if ((obj.options & ObjectOption.Bgr) != ObjectOption.Bgr && (obj.options & ObjectOption.Door) != ObjectOption.Door && (obj.options & ObjectOption.Torch) != ObjectOption.Torch && (obj.options & ObjectOption.Block) != ObjectOption.Block)
                                {
                                    if (room.selectedObject.Count == 0)
                                    {
                                        //(byte)selectedMode >= 0 && (byte)selectedMode <= 3
                                        if (selectedMode == ObjectMode.Bgallmode || (byte)selectedMode == obj.layer)
                                        {
                                            room.selectedObject.Add(obj);
                                            found = true;
                                            break;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    else //there's already objects selected
                                    {
                                        //check if the object we found is already in selected object if so do nothing
                                        //otherwise clear objects and select the new one
                                        foreach (Room_Object o in room.selectedObject)
                                        {
                                            if (o == obj)
                                            {
                                                if (selectedMode == ObjectMode.Bgallmode || (byte)selectedMode == obj.layer)
                                                {
                                                    found = true;
                                                    objectFound = o;
                                                    already_in = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                        if (already_in == false)
                                        {

                                            //objectToRemove
                                            if (ModifierKeys == Keys.Shift)
                                            {
                                                if (selectedMode == ObjectMode.Bgallmode || (byte)selectedMode == obj.layer)
                                                {
                                                    room.selectedObject.Add(obj);
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                room.selectedObject.Clear();
                                                if (selectedMode == ObjectMode.Bgallmode || (byte)selectedMode == obj.layer)
                                                {
                                                    room.selectedObject.Add(obj);
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }

                                        }
                                        else //if item is already in but we hold control then remove it instead
                                        {
                                            if (selectedMode == ObjectMode.Bgallmode || (byte)selectedMode == obj.layer)
                                            {
                                                if (ModifierKeys == Keys.Control)
                                                {
                                                    room.selectedObject.Remove(objectFound);
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (found == false) //we didnt find any Tiles to click on so just clear the selection
                        {
                            if (ModifierKeys != Keys.Shift && ModifierKeys != Keys.Control)
                            {
                                //Console.WriteLine("we didnt find any object so clear all");
                                room.selectedObject.Clear();
                            }
                        }
                    }
                    else if (selectedMode == ObjectMode.Doormode)
                    {
                        room.selectedObject.Clear();
                        dragx = ((e.X) / 8);
                        dragy = ((e.Y) / 8);
                        for (int i = room.tilesObjects.Count - 1; i >= 0; i--)
                        {
                            Room_Object obj = room.tilesObjects[i];
                            if (isMouseCollidingWith(obj, e))
                            {
                                if ((obj.options & ObjectOption.Bgr) != ObjectOption.Bgr && (obj.options & ObjectOption.Door) == ObjectOption.Door)
                                {
                                    //we found a door
                                    room.selectedObject.Add(obj);
                                    obj.selected = true;
                                    doorArray = room.getAllDoorPosition(obj);
                                    need_refresh = true;
                                    break;
                                }

                            }

                        }
                    }
                    else if (selectedMode == ObjectMode.Blockmode)
                    {
                        room.selectedObject.Clear();
                        dragx = ((e.X) / 8);
                        dragy = ((e.Y) / 8);
                        for (int i = room.tilesObjects.Count - 1; i >= 0; i--)
                        {
                            Room_Object obj = room.tilesObjects[i];
                            if (isMouseCollidingWith(obj, e))
                            {
                                if ((obj.options & ObjectOption.Bgr) != ObjectOption.Bgr && (obj.options & ObjectOption.Block) == ObjectOption.Block)
                                {
                                    room.selectedObject.Add(obj);
                                    need_refresh = true;
                                    break;
                                }

                            }

                        }
                    }
                    else if (selectedMode == ObjectMode.Torchmode)
                    {
                        room.selectedObject.Clear();
                        dragx = ((e.X) / 8);
                        dragy = ((e.Y) / 8);
                        for (int i = room.tilesObjects.Count - 1; i >= 0; i--)
                        {
                            Room_Object obj = room.tilesObjects[i];
                            if (isMouseCollidingWith(obj, e))
                            {
                                if ((obj.options & ObjectOption.Bgr) != ObjectOption.Bgr && (obj.options & ObjectOption.Torch) == ObjectOption.Torch)
                                {
                                    //we found a door
                                    room.selectedObject.Add(obj);
                                    need_refresh = true;
                                    break;
                                }

                            }

                        }
                    }
                    else if (selectedMode == ObjectMode.Warpmode)
                    {
                        room.selectedObject.Clear();
                        dragx = ((e.X) / 8);
                        dragy = ((e.Y) / 8);
                        int doorCount = 0;
                        foreach (Room_Object o in room.tilesObjects)
                        {
                            if (doorsObject.Contains(o.id))
                            {
                                if (isMouseCollidingWith(o, e))
                                {
                                    string warpid = Interaction.InputBox("New Warp Room", "Room Id", room.staircase_rooms[doorCount].ToString());
                                    byte b;
                                    if (byte.TryParse(warpid, out b))
                                    {
                                        room.staircase_rooms[doorCount] = b;
                                    }
                                    else
                                    {
                                        MessageBox.Show("The value need to be a number between 0-256");
                                    }


                                }
                                doorCount++;
                            }
                            else if (o.id == 0xFCA)
                            {
                                if (isMouseCollidingWith(o, e))
                                {
                                    string warpid = Interaction.InputBox("New Warp Room", "Room Id", room.holewarp.ToString());
                                    byte b;
                                    if (byte.TryParse(warpid, out b))
                                    {
                                        room.holewarp = b;
                                    }
                                    else
                                    {
                                        MessageBox.Show("The value need to be a number between 0-256");
                                    }

                                }
                            }
                        }
                    }
                    mouse_down = true;
                    move_x = 0;
                    move_y = 0;
                    mx = dragx;
                    my = dragy;
                    last_mx = mx;
                    last_my = my;
                }
                mainForm.spritepropertyPanel.Visible = false;
                mainForm.potitemobjectPanel.Visible = false;
                mainForm.doorselectPanel.Visible = false;
                if (room.selectedObject.Count > 0)
                {
                    if (room.selectedObject[0] is Room_Object)
                    {
                        updating_info = true;
                        Room_Object oo = room.selectedObject[0] as Room_Object;
                        if (oo.options == ObjectOption.Door)
                        {
                            string name = oo.name;
                            string id = oo.id.ToString("X4");
                            mainForm.comboBox1.Enabled = false;
                            mainForm.selectedGroupbox.Text = "Selected Object : " + id + " " + name;
                            mainForm.doorselectPanel.Visible = true;
                            int apos = mainForm.door_index.Select((s, i) => new { s, i }).Where(x => x.s == (oo as object_door).door_type).Select(x => x.i).ToArray()[0];
                            mainForm.comboBox2.SelectedIndex = apos;
                            for (int i = 0; i < room.tilesObjects.Count; i++)
                            {
                                if (room.tilesObjects[i] == oo)
                                {
                                    mainForm.selectedZUpDown.Value = i;
                                }
                            }
                            updateSelectionObject(oo);
                        }
                        else
                        {
                           

                            string name = oo.name;
                            string id = oo.id.ToString("X4");
                            mainForm.comboBox1.Enabled = false;
                            mainForm.selectedGroupbox.Text = "Selected Object : " + id + " " + name;

                            for (int i = 0; i < room.tilesObjects.Count; i++)
                            {
                                if (room.tilesObjects[i] == oo)
                                {
                                    mainForm.selectedZUpDown.Value = i;
                                }
                            }

                            updateSelectionObject(oo);
                        }
                    }
                    else if (room.selectedObject[0] is Sprite)
                    {
                        mainForm.spritepropertyPanel.Visible = true;
                        updating_info = true;
                        Sprite oo = room.selectedObject[0] as Sprite;
                        string name = oo.name;
                        string id = oo.id.ToString("X4");
                        mainForm.selectedGroupbox.Text = "Selected Sprite : " + id + " " + name;
                        mainForm.comboBox1.Enabled = true;
                        updateSelectionObject(oo);
                    }
                    else if (room.selectedObject[0] is PotItem)
                    {

                        updating_info = true;//?
                        PotItem oo = room.selectedObject[0] as PotItem;
                        mainForm.potitemobjectPanel.Visible = true; //oO why this is not appearing
                        int dropboxid = oo.id;
                        if ((oo.id & 0x80) == 0x80) //it is a special object
                        {
                            dropboxid = ((oo.id - 0x80) / 2) + 0x17; //no idea if it will work
                        }
                        //if for some reason the dropboxid >= 28
                        if (dropboxid >= 28)
                        {
                            dropboxid = 27; //prevent crash :yay:
                        }
                        string name = PotItems_Name.name[dropboxid];
                        string id = oo.id.ToString("X4");
                        mainForm.selectedGroupbox.Text = "Selected Item : " + id + " " + name;
                        mainForm.selecteditemobjectCombobox.SelectedIndex = dropboxid;

                    }
                }


            }
        }



        Rectangle lastSelectedRectangle = new Rectangle();
        bool colliding_chest = false;
        private void onMouseMove(object sender, MouseEventArgs e)
        {

                colliding_chest = false;
                if (selectedMode == ObjectMode.Chestmode)
                {
                    foreach (Chest c in room.chest_list)
                    {
                        if (e.X >= (c.x * 8) && e.Y >= (c.y * 8) - 16 && e.X <= (c.x * 8) + 16 && e.Y <= (c.y * 8) + 16)
                        {
                            mainForm.toolTip1.Show(ChestItems_Name.name[c.item] +" " + c.item.ToString("X2"), this, new Point(e.X, e.Y + 16));
                            colliding_chest = true;
                        }
                    }
                }
                if (colliding_chest == false)
                {
                    mainForm.toolTip1.Hide(this);
                }

                //Cursor.Current = Cursors.Default;
                if (room.selectedObject.Count == 1)
                {
                    if (room.selectedObject[0] is Room_Object)
                    {
                        objects_ResizeMouseMove(e); //show the arrows on selected object to resize them
                    }
                }

            if (mouse_down)
            {
                updating_info = true;





                setMouseSizeMode(e); //define the size of mx,my for each mode
                if (selectedMode != ObjectMode.Doormode)
                {
                    if (selection_resize == false)
                    {
                        if (mx != last_mx || my != last_my)
                        {
                            need_refresh = true;
                        }
                        if (room.selectedObject.Count > 0)
                        {
                            if (mx != last_mx || my != last_my)
                            {
                                move_objects();
                                room.has_changed = true;
                                last_mx = mx;
                                last_my = my;
                                updateSelectionObject(room.selectedObject[0]);
                            }
                        }

                    }
                    else
                    {
                        resizing_objects();
                    }
                }
                else //if it a door
                {
                    //TODO : Fix door draw when dragged on side position are not accurate anymore
                    if (room.selectedObject.Count > 0)
                    {
                        if (room.selectedObject[0] is object_door)
                        {
                            if (doorArray != null)
                            {
                                for (int i = 0; i < 48; i++)
                                {
                                    Rectangle r = doorArray[i];
                                    if (lastSelectedRectangle != r)
                                    {
                                        if (new Rectangle(e.X, e.Y, 1, 1).IntersectsWith(r))
                                        {
                                            lastSelectedRectangle = r;
                                            int doordir = (i / 12);
                                            if ((room.selectedObject[0] as object_door).door_pos != (byte)((i * 2) - (doordir * 12)) || (room.selectedObject[0] as object_door).door_dir != (byte)(doordir))
                                            {
                                                (room.selectedObject[0] as object_door).door_pos = (((byte)((i - (doordir * 12)) * 2)));
                                                (room.selectedObject[0] as object_door).door_dir = ((byte)(doordir));
                                                (room.selectedObject[0] as object_door).updateId();
                                                (room.selectedObject[0] as object_door).DrawOnBitmap();
                                                room.has_changed = true;
                                                room.reloadGfx();
                                                need_refresh = true;
                                            }
                                            
                                            break;

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void onMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                updating_info = false;
                selection_resize = false;

                if (mouse_down == true)
                {
                    mouse_down = false;
                    need_refresh = true;
                    if (room.selectedObject.Count == 0) //if we don't have any objects select we select what is in the rectangle
                    {
                        getObjectsRectangle();
                    }
                    else
                    {
                        setObjectsPosition();
                    }

                }
            }
            else if (e.Button == MouseButtons.Right) //that's a problem
            {
                mainForm.nothingselectedcontextMenu.Items[0].Enabled = true;
                mainForm.singleselectedcontextMenu.Items[0].Enabled = true;
                mainForm.groupselectedcontextMenu.Items[0].Enabled = true;
                string nname = "Unknown";
                if ((byte)selectedMode >= 0 && (byte)selectedMode <= 3)
                {
                    nname = "Object";
                    if (selectedMode == ObjectMode.Bgallmode)
                    {
                        mainForm.nothingselectedcontextMenu.Items[0].Enabled = false;
                        mainForm.singleselectedcontextMenu.Items[0].Enabled = false;
                        mainForm.groupselectedcontextMenu.Items[0].Enabled = false;
                    }
                }
                else if (selectedMode == ObjectMode.Spritemode)
                {
                    nname = "Sprite";
                }
                else if (selectedMode == ObjectMode.Chestmode)
                {
                    nname = "Chest Item";
                }
                else if (selectedMode == ObjectMode.Itemmode)
                {
                    nname = "Pot Item";
                }
                else if (selectedMode == ObjectMode.Blockmode)
                {
                    nname = "Block";
                }
                else if (selectedMode == ObjectMode.Torchmode)
                {
                    nname = "Torch";
                }
                else if (selectedMode == ObjectMode.Doormode)
                {
                    nname = "Door";
                }
                mainForm.nothingselectedcontextMenu.Items[0].Text = "Insert new " + nname;
                mainForm.singleselectedcontextMenu.Items[0].Text = "Insert new " + nname;
                mainForm.groupselectedcontextMenu.Items[0].Text = "Insert new " + nname;
                if (room.selectedObject.Count == 0)
                {
                    mainForm.nothingselectedcontextMenu.Show(Cursor.Position);
                }
                else if (room.selectedObject.Count == 1)
                {
                    mainForm.singleselectedcontextMenu.Show(Cursor.Position);
                }
                else if (room.selectedObject.Count > 1)
                {
                    mainForm.groupselectedcontextMenu.Show(Cursor.Position);
                }
            }

        }

        private void onMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (selectedMode == ObjectMode.Chestmode)
            {
                Chest chestToRemove = null;
                bool foundChest = false;
                foreach (Chest c in room.chest_list)
                {
                    if (e.X >= (c.x * 8) && e.X <= (c.x * 8) + 16 &&
                        e.Y >= ((c.y - 2) * 8) && e.Y <= ((c.y) * 8) + 16)
                    {
                        chestpicker.button3.Enabled = true; 
                        DialogResult result = chestpicker.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            //change chest item
                            c.item = (byte)chestpicker.listView1.SelectedIndices[0];
                            room.has_changed = true;
                        }
                        else if (result == DialogResult.No)
                        {
                            chestToRemove = c;
                        }
                        foundChest = true;
                        break;
                    }
                }
                if (foundChest == false)
                {
                    chestpicker.button3.Enabled = false;
                    DialogResult result = chestpicker.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        room.has_changed = true;
                        //change chest item
                        Chest c = new Chest((byte)(e.X / 8), (byte)(e.Y / 8), (byte)chestpicker.listView1.SelectedIndices[0], false, false);
                        room.chest_list.Add(c);
                    }
                }

                if (chestToRemove != null)
                {
                    room.chest_list.Remove(chestToRemove);
                }

                need_refresh = true;
            }
        }

        public void move_objects()
        {
            foreach (Object o in room.selectedObject)
            {
                if (o is Sprite)
                {
                    (o as Sprite).nx = (byte)((o as Sprite).x + move_x);
                    (o as Sprite).ny = (byte)((o as Sprite).y + move_y);
                    if ((o as Sprite).nx > 80)
                    {
                        (o as Sprite).nx = 0;
                    }
                    if ((o as Sprite).ny > 80)
                    {
                        (o as Sprite).ny = 0;
                    }
                }
                else if (o is PotItem)
                {
                    (o as PotItem).nx = (byte)((o as PotItem).x + move_x);
                    (o as PotItem).ny = (byte)((o as PotItem).y + move_y);
                    if ((o as PotItem).nx > 80)
                    {
                        (o as PotItem).nx = 0;
                    }
                    if ((o as PotItem).ny > 80)
                    {
                        (o as PotItem).ny = 0;
                    }
                }
                else if (o is Room_Object)
                {
                    (o as Room_Object).nx = (byte)((o as Room_Object).x + move_x);
                    (o as Room_Object).ny = (byte)((o as Room_Object).y + move_y);
                    if ((o as Room_Object).nx > 80)
                    {
                        (o as Room_Object).nx = 0;
                    }
                    if ((o as Room_Object).ny > 80)
                    {
                        (o as Room_Object).ny = 0;
                    }
                }
            }



        }

        public void clearUselessRoomStuff(Room r)
        {
            foreach (Object o in r.tilesObjects)
            {
                (o as Room_Object).bitmap = null;
            }

        }

        public void setObjectsPosition()
        {
            if (room.selectedObject.Count > 0)
            {
                //Room r = (Room)room.Clone();
                //clearUselessRoomStuff(r);
                //undoRooms.Add(r); //TODO : Global Undo thing?

                if (selectedMode == ObjectMode.Spritemode)
                {
                    foreach (Object o in room.selectedObject)
                    {
                        (o as Sprite).x = (o as Sprite).nx;
                        (o as Sprite).y = (o as Sprite).ny;
                        //(o as Sprite).boundingbox
                    }
                }
                else if (selectedMode == ObjectMode.Itemmode)
                {
                    foreach (Object o in room.selectedObject)
                    {
                        (o as PotItem).x = (o as PotItem).nx;
                        (o as PotItem).y = (o as PotItem).ny;
                    }
                }
                else if ((byte)selectedMode >= 0 && (byte)selectedMode <= 3) //nop nevermind
                {
                    foreach (Object o in room.selectedObject)
                    {
                        (o as Room_Object).x = (o as Room_Object).nx;
                        (o as Room_Object).y = (o as Room_Object).ny;
                        (o as Room_Object).ox = (o as Room_Object).x;
                        (o as Room_Object).oy = (o as Room_Object).y;
                        (o as Room_Object).savedSize = (o as Room_Object).size;
                        (o as Room_Object).oldSize = (o as Room_Object).size;
                    }
                }
                else if (selectedMode == ObjectMode.Torchmode)
                {
                    foreach (Object o in room.selectedObject)
                    {
                        (o as Room_Object).x = (o as Room_Object).nx;
                        (o as Room_Object).y = (o as Room_Object).ny;
                        (o as Room_Object).ox = (o as Room_Object).x;
                        (o as Room_Object).oy = (o as Room_Object).y;
                        (o as Room_Object).savedSize = (o as Room_Object).size;
                        (o as Room_Object).oldSize = (o as Room_Object).size;
                    }
                }
                else if (selectedMode == ObjectMode.Blockmode)
                {
                    foreach (Object o in room.selectedObject)
                    {
                        (o as Room_Object).x = (o as Room_Object).nx;
                        (o as Room_Object).y = (o as Room_Object).ny;
                        (o as Room_Object).ox = (o as Room_Object).x;
                        (o as Room_Object).oy = (o as Room_Object).y;
                        (o as Room_Object).savedSize = (o as Room_Object).size;
                        (o as Room_Object).oldSize = (o as Room_Object).size;
                    }
                }
                
                need_refresh = true;
                //redoRooms.Clear(); //TODO change undo redo stuff... ... ... change it into scene directly instead of global !
            }
        }

        public void objects_ResizeMouseMove(MouseEventArgs e)
        {
            int rightBorder = ((room.selectedObject[0] as Room_Object).x * 8) + (room.selectedObject[0] as Room_Object).width;
            int bottomBorder = ((room.selectedObject[0] as Room_Object).y * 8) + (room.selectedObject[0] as Room_Object).height;
            int leftBorder = ((room.selectedObject[0] as Room_Object).x * 8);
            int topBorder = ((room.selectedObject[0] as Room_Object).y * 8);

            if (mouse_down == false)
            {
                resizing = ObjectResize.None;
                Room_Object obj = (room.selectedObject[0] as Room_Object);
                if ((obj.id >= 0x00 && obj.id <= 0x5F) || (obj.id >= 0xA0 && obj.id <= 0xBF) || (obj.id >= 0xC0 && obj.id <= 0xF7)) //horizontally scrollable
                {
                    if (e.X >= (rightBorder - 2) && e.X <= (rightBorder + 4) && //right
                        e.Y >= topBorder + 1 && e.Y <= bottomBorder - 1)
                    {
                        Cursor.Current = Cursors.SizeWE;
                        resizing = ObjectResize.Right;

                    }
                }
                /*else if (e.X >= (leftBorder - 4) && e.X <= (leftBorder + 2) && //left
                    e.Y >= topBorder + 2 && e.Y <= bottomBorder - 2)
                {
                    Cursor.Current = Cursors.SizeWE;
                    resizing = ObjectResize.Left;
                }
                else if (e.X >= (leftBorder + 4) && e.X <= (rightBorder - 4) && //up
                    e.Y >= topBorder - 4 && e.Y <= topBorder + 2)
                {
                    Cursor.Current = Cursors.SizeNS;
                    resizing = ObjectResize.Up;
                }*/
                if ((obj.id >= 0x60 && obj.id <= 0x96) || (obj.id >= 0xA0 && obj.id <= 0xAF) || (obj.id >= 0xC0 && obj.id <= 0xF7))
                {
                    if (e.X >= (leftBorder + 1) && e.X <= (rightBorder - 1) && //down 
                    e.Y >= bottomBorder - 2 && e.Y <= bottomBorder + 4)
                    {
                        Cursor.Current = Cursors.SizeNS;
                        resizing = ObjectResize.Down;
                    }
                }
                /*else if (e.X >= (leftBorder - 4) && e.X <= (leftBorder + 2) && //diagonal up left
                    e.Y >= topBorder - 4 && e.Y <= topBorder + 2)
                {
                    Cursor.Current = Cursors.SizeNWSE;
                    resizing = ObjectResize.UpLeft;
                }
                else if (e.X >= (rightBorder - 2) && e.X <= (rightBorder + 4) && //diagonal bottom right
                    e.Y >= bottomBorder - 2 && e.Y <= bottomBorder + 4)
                {
                    Cursor.Current = Cursors.SizeNWSE;
                    resizing = ObjectResize.DownRight;
                }
                else if (e.X >= (rightBorder - 2) && e.X <= (rightBorder + 4) && //diagonal up right
                    e.Y >= topBorder - 4 && e.Y <= topBorder + 2)
                {
                    Cursor.Current = Cursors.SizeNESW;
                    resizing = ObjectResize.UpRight;
                }
                else if (e.X >= (leftBorder - 4) && e.X <= (leftBorder + 2) && //diagonal bottom left
                    e.Y >= bottomBorder - 2 && e.Y <= bottomBorder + 4)
                {
                    Cursor.Current = Cursors.SizeNESW;
                    resizing = ObjectResize.DownLeft;
                }*/
            }
            else
            {
                if (resizing != ObjectResize.None)
                {
                    if (resizing == ObjectResize.Right)
                    {
                        Cursor.Current = Cursors.SizeWE;
                        dragx = (room.selectedObject[0] as Room_Object).x;
                    }
                    else if (resizing == ObjectResize.Down)
                    {
                        Cursor.Current = Cursors.SizeNS;
                        dragy = (room.selectedObject[0] as Room_Object).y;
                    }
                }
            }
        }

        public void setMouseSizeMode(MouseEventArgs e)
        {
            if (selectedMode == ObjectMode.Spritemode)
            {
                mx = ((e.X) / 16);
                my = ((e.Y) / 16);
            }
            else if (selectedMode == ObjectMode.Itemmode)
            {
                mx = ((e.X) / 8);
                my = ((e.Y) / 8);
            }
            else if ((byte)selectedMode >= 0 && (byte)selectedMode <= 3)
            {
                mx = ((e.X) / 8);
                my = ((e.Y) / 8);
            }
            else if (selectedMode == ObjectMode.Torchmode || selectedMode == ObjectMode.Blockmode)
            {
                mx = ((e.X) / 8);
                my = ((e.Y) / 8);
            }

            move_x = mx - dragx; //number of tiles mouse is compared to starting drag point X
            move_y = my - dragy; //number of tiles mouse is compared to starting drag point Y
        }

        public void resizing_objects()
        {
            if (mx != last_mx || my != last_my)
            {
                //TODO : Finish resizing objects, only right side is working !
                room.has_changed = true; //will prompt room has changed dialog
                last_mx = mx;
                last_my = my;
                need_refresh = true;
                Room_Object obj = (room.selectedObject[0] as Room_Object);
                //move_x = nbr of tiles the mouse moved x axis from drag
                //move_y = nbr of tiles the mouse moved y axis from drag
                if (resizing == ObjectResize.Right)
                {

                    //0C0 to 0F7

                    if ((obj.id >= 0x00 && obj.id <= 0x5F) || (obj.id >= 0xA0 && obj.id <= 0xBF) || (obj.id >= 0xC0 && obj.id <= 0xF7)) //horizontally scrollable
                    {
                        byte w = obj.base_width;
                        if ((obj.id >= 0xC0 && obj.id <= 0xF7))
                        {
                            if (move_x > w - 1)
                            {
                                byte sizeX = (byte)((move_x - w) / obj.scroll_x);

                                if ((sizeX >= 03))
                                {
                                    sizeX = 03;
                                }
                                else if (sizeX <= 0)
                                {
                                    sizeX = 0;
                                }
                                obj.size = (byte)(((sizeX & 03) << 2) + (obj.size & 0x03));
                                if (obj.oldSize != obj.size)
                                {
                                    obj.resetSize();
                                    obj.DrawOnBitmap();
                                    obj.oldSize = obj.size;
                                }
                            }
                        }
                        else
                        {
                            if (obj.special_zero_size != 0)
                            {
                                w = 0;
                            }
                            if (move_x > w - 1)
                            {
                                obj.size = (byte)((move_x - w) / obj.scroll_x);

                                if ((obj.size >= 15))
                                {
                                    if (obj.special_zero_size != 0)
                                    {
                                        obj.size = 0;
                                    }
                                    else
                                    {
                                        obj.size = 15;
                                    }
                                }
                                else if (obj.size <= 0)
                                {
                                    if (obj.special_zero_size != 0)
                                    {
                                        obj.size = 1;
                                    }
                                    else
                                    {
                                        obj.size = 0;
                                        obj.DrawOnBitmap();
                                    }
                                }

                                if (obj.oldSize != obj.size)
                                {
                                    obj.resetSize();
                                    obj.DrawOnBitmap();
                                    obj.oldSize = obj.size;
                                }
                            }
                            else if (move_x < obj.base_width)
                            {
                                if (obj.special_zero_size != 0)
                                {
                                    obj.size = 1;
                                }
                                else
                                {
                                    obj.size = 0;
                                    obj.DrawOnBitmap();
                                }
                            }
                        }
                    }

                }
                else if (resizing == ObjectResize.Down)
                {
                    if ((obj.id >= 0x60 && obj.id <= 0x96) || (obj.id >= 0xA0 && obj.id <= 0xAF) || (obj.id >= 0xC0 && obj.id <= 0xF7)) //vertically scrollable
                    {
                        byte h = obj.base_height;
                        //Objects that are resizable from both side
                        if ((obj.id >= 0xC0 && obj.id <= 0xF7))
                        {

                            if (move_y > h - 1)
                            {
                                byte sizeY = (byte)((move_y - h) / obj.scroll_y);

                                if ((sizeY >= 03))
                                {
                                    sizeY = 03;
                                }
                                else if (sizeY <= 0)
                                {
                                    sizeY = 0;
                                }
                                obj.size = (byte)(((obj.size & 0x0C)) + (sizeY & 03));
                                if (obj.oldSize != obj.size)
                                {
                                    obj.resetSize();
                                    obj.DrawOnBitmap();
                                    obj.oldSize = obj.size;
                                }
                            }
                        }
                        else //Object that are just resizable on Y axis
                        {

                            if (obj.special_zero_size != 0)
                            {
                                h = 0;
                            }
                            if (move_y > h - 1)
                            {
                                obj.size = (byte)((move_y - h) / obj.scroll_y);

                                if ((obj.size >= 15))
                                {
                                    if (obj.special_zero_size != 0)
                                    {
                                        obj.size = 0;
                                    }
                                    else
                                    {
                                        obj.size = 15;
                                    }
                                }
                                else if (obj.size <= 0)
                                {
                                    if (obj.special_zero_size != 0)
                                    {
                                        obj.size = 1;
                                    }
                                    else
                                    {
                                        obj.size = 0;
                                        obj.DrawOnBitmap();
                                    }
                                }

                                if (obj.oldSize != obj.size)
                                {
                                    obj.resetSize();
                                    obj.DrawOnBitmap();
                                    obj.oldSize = obj.size;
                                }
                            }
                            else if (move_y < obj.base_height)
                            {
                                if (obj.special_zero_size != 0)
                                {
                                    obj.size = 1;
                                }
                                else
                                {
                                    obj.size = 0;
                                    obj.DrawOnBitmap();
                                }
                            }
                        }


                        /*

                        if (obj.special_zero_size != 0)
                        {
                            h = 0;
                        }
                        if (move_y > h - 1)
                        {
                            obj.size = (byte)((move_y - h) / obj.scroll_y);

                            if ((obj.size >= 15))
                            {
                                if (obj.special_zero_size != 0)
                                {
                                    obj.size = 0;
                                }
                                else
                                {
                                    obj.size = 15;
                                }
                            }
                            else if (obj.size <= 0)
                            {
                                if (obj.special_zero_size != 0)
                                {
                                    obj.size = 1;
                                }
                                else
                                {
                                    obj.size = 0;
                                    obj.DrawOnBitmap();
                                }
                            }

                            if (obj.oldSize != obj.size)
                            {
                                obj.resetSize();
                                obj.DrawOnBitmap();
                                obj.oldSize = obj.size;
                            }
                        }
                        else if (move_y < obj.base_width)
                        {
                            if (obj.special_zero_size != 0)
                            {
                                obj.size = 1;
                            }
                            else
                            {
                                obj.size = 0;
                                obj.DrawOnBitmap();
                            }
                        }
                    }*/
                    }
                }
                updating_info = true;
                updateSelectionObject(obj);
            }
        }









        public bool isMouseCollidingWith(Object o, MouseEventArgs e)
        {
            if (o is Sprite)
            {
                if (e.X >= (o as Sprite).boundingbox.X && e.X <= (o as Sprite).boundingbox.X + (o as Sprite).boundingbox.Width &&
                e.Y >= (o as Sprite).boundingbox.Y && e.Y <= (o as Sprite).boundingbox.Y + (o as Sprite).boundingbox.Height)
                {
                    return true;
                }
            }
            else if (o is PotItem)
            {
                if (e.X >= ((o as PotItem).x * 8) && e.X <= ((o as PotItem).x * 8) + 16 &&
                    e.Y >= ((o as PotItem).y * 8) && e.Y <= ((o as PotItem).y * 8) + 16)
                {
                    return true;
                }
            }
            else if (o is Room_Object)
            {
                if (e.X >= ((o as Room_Object).x * 8) && e.X <= (((o as Room_Object).x) * 8) + (o as Room_Object).width &&
                e.Y >= (((o as Room_Object).y + (o as Room_Object).drawYFix) * 8) && e.Y <= ((((o as Room_Object).y + (o as Room_Object).drawYFix)) * 8) + (o as Room_Object).height)
                {
                    return true;
                }
            }
            return false;
        }

        public void getObjectsRectangle()
        {
            if (room.selectedObject.Count == 0)
            {
                if (selectedMode == ObjectMode.Spritemode) //we're looking for sprites
                {
                    foreach (Sprite spr in room.sprites)
                    {
                        int rx = dragx;
                        int ry = dragy;
                        if (move_x < 0) { Math.Abs(rx = dragx + move_x); }
                        if (move_y < 0) { Math.Abs(ry = dragy + move_y); }

                        if (spr.boundingbox.IntersectsWith(new Rectangle(rx * 16, ry * 16, Math.Abs(move_x) * 16, Math.Abs(move_y) * 16)))
                        {
                            room.selectedObject.Add(spr);
                        }
                    }
                }
                else if (selectedMode == ObjectMode.Itemmode)//we're looking for pot items
                {
                    foreach (PotItem item in room.pot_items)
                    {
                        int rx = dragx;
                        int ry = dragy;
                        if (move_x < 0) { Math.Abs(rx = dragx + move_x); }
                        if (move_y < 0) { Math.Abs(ry = dragy + move_y); }

                        if ((new Rectangle(item.x * 8, item.y * 8, 16, 16)).IntersectsWith(new Rectangle(rx * 8, ry * 8, Math.Abs(move_x) * 8, Math.Abs(move_y) * 8)))
                        {
                            room.selectedObject.Add(item);
                        }
                    }
                }
                else if ((byte)selectedMode >= 0 && (byte)selectedMode <= 3)//we're looking for tiles
                {

                    foreach (Room_Object o in room.tilesObjects)
                    {
                        int rx = dragx;
                        int ry = dragy;
                        if (move_x < 0) { Math.Abs(rx = dragx + move_x); }
                        if (move_y < 0) { Math.Abs(ry = dragy + move_y); }

                        if ((new Rectangle((o as Room_Object).x * 8, ((o as Room_Object).y + (o as Room_Object).drawYFix) * 8, (o as Room_Object).width, (o as Room_Object).height)).IntersectsWith(new Rectangle(rx * 8, ry * 8, Math.Abs(move_x) * 8, Math.Abs(move_y) * 8)))
                        {
                            if ((o.options & ObjectOption.Bgr) != ObjectOption.Bgr && (o.options & ObjectOption.Door) != ObjectOption.Door && (o.options & ObjectOption.Torch) != ObjectOption.Torch && (o.options & ObjectOption.Block) != ObjectOption.Block)
                            {
                                if (selectedMode == ObjectMode.Bgallmode)
                                {
                                    room.selectedObject.Add(o);
                                }
                                else
                                {
                                    if ((byte)selectedMode == o.layer)
                                    {
                                        room.selectedObject.Add(o);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {

                }
            }

        }

        public void updateSelectionObject(object o)
        {
            if (room.selectedObject.Count == 1)
            {
                if (o is Room_Object)
                {

                    mainForm.selectedXNumericUpDown.Maximum = 64;
                    mainForm.selectedYNumericUpDown.Maximum = 64;
                    mainForm.selectedSizeNumericUpDown.Maximum = 16;
                    mainForm.selectedLayerNumericUpDown.Maximum = 2;
                    if ((o as Room_Object).nx >= 63)
                    {
                        (o as Room_Object).nx = 63;
                    }
                    if ((o as Room_Object).ny >= 63)
                    {
                        (o as Room_Object).ny = 63;
                    }
                    if ((o as Room_Object).size >= 16)
                    {
                        (o as Room_Object).size = 0;
                    }
                    if ((o as Room_Object).layer >= 3)
                    {
                        (o as Room_Object).size = 2;
                    }


                    mainForm.selectedXNumericUpDown.Value = (o as Room_Object).nx;
                    mainForm.selectedYNumericUpDown.Value = (o as Room_Object).ny;
                    mainForm.selectedSizeNumericUpDown.Value = (o as Room_Object).size;
                    mainForm.selectedLayerNumericUpDown.Value = (o as Room_Object).layer;
                    
                    /*
                    if ((o.options & ObjectOption.Door) == ObjectOption.Door)
                    {
                        byte door_pos = (byte)((o.id & 0xF0) >> 3);
                        byte door_dir = (byte)((o.id & 0x03));
                        byte door_type = (byte)((o.id >> 8) & 0xFF);
                        id = o.id.ToString("X4") + "\nDoor Type : " + door_type.ToString("X2");
                        id += "\nDoor Direction : " + door_dir.ToString("X2");
                        id += "\nDoor Position : " + door_pos.ToString("X2");
                    }
                    if ((o.options & ObjectOption.Chest) == ObjectOption.Chest)
                    {
                        id += "\nBig Chest ? : " + room.chest_list[0].bigChest.ToString();
                    }*/

                }
                else if (o is object_door)
                {
                    mainForm.selectedXNumericUpDown.Maximum = 64;
                    mainForm.selectedYNumericUpDown.Maximum = 64;
                    mainForm.selectedSizeNumericUpDown.Maximum = 16;
                    mainForm.selectedLayerNumericUpDown.Maximum = 2;
                    if ((o as object_door).nx >= 63)
                    {
                        (o as object_door).nx = 63;
                    }
                    if ((o as object_door).ny >= 63)
                    {
                        (o as object_door).ny = 63;
                    }
                    if ((o as object_door).size >= 16)
                    {
                        (o as object_door).size = 0;
                    }
                    if ((o as object_door).layer >= 3)
                    {
                        (o as object_door).size = 2;
                    }


                    mainForm.selectedXNumericUpDown.Value = (o as object_door).nx;
                    mainForm.selectedYNumericUpDown.Value = (o as object_door).ny;
                    mainForm.selectedSizeNumericUpDown.Value = (o as object_door).size;
                    mainForm.selectedLayerNumericUpDown.Value = (o as object_door).layer;
                }
                else if (o is Sprite)
                {

                    mainForm.selectedXNumericUpDown.Maximum = 32;
                    mainForm.selectedYNumericUpDown.Maximum = 32;
                    mainForm.selectedLayerNumericUpDown.Maximum = 1;
                    if ((o as Sprite).nx >= 31)
                    {
                        (o as Sprite).nx = 31;
                    }
                    if ((o as Sprite).ny >= 31)
                    {
                        (o as Sprite).ny = 31;
                    }
                    if ((o as Sprite).layer >= 2)
                    {
                        (o as Sprite).layer = 1;
                    }

                    mainForm.selectedXNumericUpDown.Value = (o as Sprite).nx;
                    mainForm.selectedYNumericUpDown.Value = (o as Sprite).ny;
                    mainForm.selectedLayerNumericUpDown.Value = (o as Sprite).layer;
                    mainForm.comboBox1.SelectedIndex = (o as Sprite).keyDrop;

 
                    //info = name + "\nId : " + id + "\nX : " + x + "\nY : " + y + "\nLayer : " + layer;
                }
                else if (o is PotItem)
                {
                    mainForm.selectedXNumericUpDown.Maximum = 64;
                    mainForm.selectedYNumericUpDown.Maximum = 64;
                    mainForm.selectedLayerNumericUpDown.Maximum = 1;
                    if ((o as PotItem).nx >= 63)
                    {
                        (o as PotItem).nx = 63;
                    }
                    if ((o as PotItem).ny >= 63)
                    {
                        (o as PotItem).ny = 63;
                    }
                    if ((o as PotItem).layer >= 2) //NVM
                    {
                        (o as PotItem).layer = 1;
                    }

                    mainForm.selectedXNumericUpDown.Value = (o as PotItem).nx;
                    mainForm.selectedYNumericUpDown.Value = (o as PotItem).ny;
                    mainForm.selectedLayerNumericUpDown.Value = (o as PotItem).layer;

                }
            }
        }



        public override void drawRoom()
        {
            if (room == null)
            {
                //Console.WriteLine("Problem!");
                //need_refresh = false;
                return;
            }

            if (room.needGfxRefresh)
            {
                room.reloadGfx();
                room.needGfxRefresh = false;
            }

            if (need_refresh)
            {
                //updateSelectionObject();
                addSpecialErasedDraw();
                drawLayout();
                drawLayer1and3plusDoors();
                drawLayer2();
                drawLayersOnBgr();
                if (!mainForm.hideChestItemsToolStripMenuItem.Checked)
                {
                    drawChests();
                }
                if (!mainForm.hideSpritesToolStripMenuItem.Checked)
                {
                    drawSprites();
                }
                if (!mainForm.hideItemsToolStripMenuItem.Checked)
                {
                    GFX.begin_draw(scene_bitmap);
                    room.drawPotsItems();
                    GFX.end_draw(scene_bitmap);
                }
                if (!mainForm.hideAllTextToolStripMenuItem.Checked)
                {
                    drawWarp();
                }
                drawGrid();
                drawSelection();
                drawEntrancePosition();
                drawDoorsPosition();
                this.Image = scene_bitmap;
                this.Refresh();
                need_refresh = false;
            }

        }







        public void Undo()
        {
            selection_resize = false;
            room.selectedObject.Clear();

        }

        public void Redo()
        {

        }

        public override void selectAll()
        {
            room.selectedObject.Clear();
            if (selectedMode == ObjectMode.Spritemode)
            {
                foreach (Sprite spr in room.sprites)
                {
                    room.selectedObject.Add(spr);
                }
            }
            foreach (Room_Object o in room.tilesObjects)
            {
                if (o.options == ObjectOption.Nothing)
                {
                    if ((byte)selectedMode <= 3)
                    {
                        if (selectedMode == ObjectMode.Bgallmode)
                        {
                            room.selectedObject.Add(o);
                        }
                        else
                        {
                            if ((byte)selectedMode == o.layer)
                            {
                                room.selectedObject.Add(o);
                            }
                        }
                    }

                }
            }
            need_refresh = true;
        }

        public override void deleteSelected()
        {

            room.has_changed = true;
            foreach (Object o in room.selectedObject)
            {
                if (o is Room_Object)
                {
                    room.tilesObjects.Remove((o as Room_Object));
                }
                else if (o is Sprite)
                {
                    room.sprites.Remove((o as Sprite));
                }
                else if (o is PotItem)
                {
                    room.pot_items.Remove((o as PotItem));
                }
            }
            resizing = ObjectResize.None;
            selection_resize = false;
            //redoRooms.Clear();
            room.selectedObject.Clear();
            need_refresh = true;

        }

        public override void paste()
        {
            List<SaveObject> data = (List<SaveObject>)Clipboard.GetData("ObjectZ");
            if (data != null)
            {
                if (data.Count > 0)
                {
                    int most_x = 512;
                    int most_y = 512;
                    foreach (SaveObject o in data)
                    {
                        if (data.Count > 0)
                        {
                            if (o.x < most_x)
                            {
                                most_x = o.x;
                            }
                            if (o.y < most_y)
                            {
                                most_y = o.y;
                            }
                        }
                        else
                        {
                            most_x = 0;
                            most_y = 0;
                        }
                    }
                    room.selectedObject.Clear();

                    foreach (SaveObject o in data)
                    {
                        if (o.type == typeof(Sprite))
                        {
                            Sprite spr = (new Sprite(room, o.id, (byte)(o.x - most_x), (byte)(o.y - most_y), Sprites_Names.name[o.id], o.overlord, o.subtype, o.layer));
                            room.sprites.Add(spr);
                            room.selectedObject.Add(spr);
                        }
                        else if (o.type == typeof(Room_Object))
                        {
                            if ((o.options & ObjectOption.Door) == ObjectOption.Door)
                            {
                                object_door ro = new object_door(o.tid, o.x, o.y, 0, o.layer);
                                ro.setRoom(room);
                                ro.options = (ObjectOption)o.options;
                                room.tilesObjects.Add(ro);
                                room.selectedObject.Add(ro);

                            }
                            else
                            {
                                Room_Object ro = room.addObject(o.tid, (byte)(o.x - most_x), (byte)(o.y - most_y), o.size, o.layer);
                                if (ro != null)
                                {
                                    ro.setRoom(room);
                                    ro.options = (ObjectOption)o.options;
                                    room.tilesObjects.Add(ro);
                                    room.selectedObject.Add(ro);
                                }
                            }
                        }
                        else if (o.type == typeof(PotItem))
                        {
                            PotItem item = (new PotItem((byte)o.tid, (byte)(o.x - most_x), (byte)(o.y - most_y), (o.layer == 1 ? true : false)));
                            room.pot_items.Add(item);
                            room.selectedObject.Add(item);
                        }
                    }

                    dragx = 0;
                    dragy = 0;
                    mouse_down = true;
                    need_refresh = true;
                    room.reloadGfx(false);
                }
            }
        }

        public override void copy()
        {
            Clipboard.Clear();
            List<SaveObject> odata = new List<SaveObject>();
            foreach (Object o in room.selectedObject)
            {
                if (o is Sprite)
                {
                    odata.Add(new SaveObject((Sprite)o));
                }
                if (o is PotItem)
                {
                    odata.Add(new SaveObject((PotItem)o));
                }
                if (o is Room_Object)
                {
                    odata.Add(new SaveObject((Room_Object)o));
                }
            }
            Clipboard.SetData("ObjectZ", odata);
        }


        public override void loadLayout()
        {
            //Room r = (Room)room.Clone();
            //clearUselessRoomStuff(r);
            //undoRooms.Add(r);

            string f = Interaction.InputBox("Name of the layout to load", "Name?", "Layout00");
            BinaryReader br = new BinaryReader(new FileStream("Layout\\" + f, FileMode.Open, FileAccess.Read));

            string type = br.ReadString();
            List<SaveObject> data = new List<SaveObject>();
            while (br.BaseStream.Position != br.BaseStream.Length)
            {
                data.Add(new SaveObject(br, typeof(Room_Object)));
            }

            if (data.Count > 0)
            {
                int most_x = 512;
                int most_y = 512;
                foreach (SaveObject o in data)
                {
                    if (data.Count > 0)
                    {
                        if (o.x < most_x)
                        {
                            most_x = o.x;
                        }
                        if (o.y < most_y)
                        {
                            most_y = o.y;
                        }
                    }
                    else
                    {
                        most_x = 0;
                        most_y = 0;
                    }
                }
                room.selectedObject.Clear();

                foreach (SaveObject o in data)
                {
                    if (o.type == typeof(Sprite))
                    {
                        Sprite spr = (new Sprite(room, o.id, (byte)(o.x - most_x), (byte)(o.y - most_y), Sprites_Names.name[o.id], o.overlord, o.subtype, o.layer));
                        room.sprites.Add(spr);
                        room.selectedObject.Add(spr);
                    }
                    else if (o.type == typeof(Room_Object))
                    {
                        Room_Object ro = room.addObject(o.tid, (byte)(o.x - most_x), (byte)(o.y - most_y), o.size, o.layer);
                        if (ro != null)
                        {
                            ro.setRoom(room);
                            ro.options = o.options;
                            room.tilesObjects.Add(ro);
                            room.selectedObject.Add(ro);
                        }
                    }
                }

                dragx = 0;
                dragy = 0;
                mouse_down = true;
                need_refresh = true;
                room.reloadGfx(false);
            }
        }


        public override void cut()
        {
            Clipboard.Clear();
            //Room r = (Room)room.Clone();
            //clearUselessRoomStuff(r);
            room.has_changed = true;
            //undoRooms.Add(r);

            List<SaveObject> odata = new List<SaveObject>();
            foreach (Object o in room.selectedObject)
            {
                if (o is Sprite)
                {
                    odata.Add(new SaveObject((Sprite)o));
                }
                if (o is PotItem)
                {
                    odata.Add(new SaveObject((PotItem)o));
                }
                if (o is Room_Object)
                {
                    odata.Add(new SaveObject((Room_Object)o));
                }

            }
            Clipboard.SetData("ObjectZ", odata);

            foreach (Object o in room.selectedObject)
            {
                if (o is Sprite)
                {
                    room.sprites.Remove((Sprite)o);
                }
                if (o is PotItem)
                {
                    room.pot_items.Remove((PotItem)o);
                }
                if (o is Room_Object)
                {
                    room.tilesObjects.Remove((Room_Object)o);
                }
            }
            room.selectedObject.Clear();
            need_refresh = true;
            //redoRooms.Clear();
        }

        public override void insertNew()
        {
            //if block selected
            if (selectedMode == ObjectMode.Blockmode)
            {
                room.selectedObject.Clear();
                Room_Object ro = room.addObject(0x0E00, (byte)0, (byte)0, 0, 0);
                if (ro != null)
                {
                    ro.setRoom(room);
                    ro.options = ObjectOption.Block;
                    room.tilesObjects.Add(ro);
                    room.selectedObject.Add(ro);
                    dragx = 0;
                    dragy = 0;
                    mouse_down = true;
                    need_refresh = true;
                    room.reloadGfx(false);
                }
            }
            else if (selectedMode == ObjectMode.Itemmode)
            {
                room.selectedObject.Clear();
                PotItem p = new PotItem(1, 0, 0, false);
                room.pot_items.Add(p);
                    room.selectedObject.Add(p);
                    dragx = 0;
                    dragy = 0;
                    mouse_down = true;
                    need_refresh = true;
                    room.reloadGfx(false);
            }
            else if (selectedMode == ObjectMode.Torchmode)
            {
                room.selectedObject.Clear();
                Room_Object ro = room.addObject(0x150, (byte)0, (byte)0, 0, 0);
                if (ro != null)
                {
                    ro.setRoom(room);
                    ro.options = ObjectOption.Torch;
                    room.tilesObjects.Add(ro);
                    room.selectedObject.Add(ro);
                    dragx = 0;
                    dragy = 0;
                    mouse_down = true;
                    need_refresh = true;
                    room.reloadGfx(false);
                }
            }
            else if (selectedMode == ObjectMode.Doormode)
            {
                room.selectedObject.Clear();
                Room_Object ro = new object_door(0, 0, 0, 0, 0);
                if (ro != null)
                {
                    ro.setRoom(room);
                    ro.options = ObjectOption.Door;
                    room.tilesObjects.Add(ro);
                    room.selectedObject.Add(ro);
                    need_refresh = true;
                    room.reloadGfx(false);
                }
            }
            else if ((byte)selectedMode >= 0 && (byte)selectedMode < 3) //if != allbg
            {
                //picker object :thinking:
                pObj.createObjects(room);
                pObj.ShowDialog();
                if (pObj.selectedObject != -1)
                {
                    room.selectedObject.Clear();
                    Room_Object ro = room.addObject(pObj.selectedObject, (byte)0, (byte)0, 0, (byte)selectedMode);
                    if (ro != null)
                    {
                        ro.setRoom(room);
                        ro.get_scroll_x();
                        ro.get_scroll_y();
                        if (ro.special_zero_size != 0)
                        {
                            ro.size = 1;
                        }

                        
                        room.tilesObjects.Add(ro);
                        room.selectedObject.Add(ro);
                        dragx = 0;
                        dragy = 0;
                        mouse_down = true;
                        need_refresh = true;
                        room.reloadGfx();
                    }

                }

            }
        }

        public override void SendSelectedToBack()
        {
            if (room.selectedObject.Count > 0)
            {
                if (room.selectedObject[0] is Room_Object)
                {
                    foreach (Room_Object o in room.selectedObject)
                    {
                        for (int i = 0; i < room.tilesObjects.Count; i++)
                        {

                            if (o == room.tilesObjects[i])
                            {
                                room.tilesObjects.RemoveAt(i);
                                room.tilesObjects.Insert(0, o);
                                break;
                            }
                        }
                    }
                }
                need_refresh = true;
            }
        }

        public override void DecreaseSelectedZ()
        {

            if (room.selectedObject.Count > 0)
            {
                if (room.selectedObject[0] is Room_Object)
                {
                    foreach (Room_Object o in room.selectedObject)
                    {
                        for (int i = 0; i < room.tilesObjects.Count; i++)
                        {

                            if (o == room.tilesObjects[i])
                            {
                                if (i > 0)
                                {
                                    room.tilesObjects.RemoveAt(i);
                                    room.tilesObjects.Insert(i - 1, o);
                                }
                                break;
                            }
                        }
                    }
                }
                need_refresh = true;
            }
        }

        public override void UpdateSelectedZ(int position)
        {
            if (room.selectedObject.Count > 0)
            {
                if (room.selectedObject[0] is Room_Object)
                {
                    foreach (Room_Object o in room.selectedObject)
                    {
                        for (int i = 0; i < room.tilesObjects.Count; i++)
                        {

                            if (o == room.tilesObjects[i])
                            {
                                if (i < room.tilesObjects.Count-1)
                                {
                                    room.tilesObjects.RemoveAt(i);
                                    room.tilesObjects.Insert(position, o);
                                }
                                break;
                            }
                        }
                    }
                }
                need_refresh = true;
            }
        }

        public override void changeObject()
        {
            if ((byte)selectedMode >= 0 && (byte)selectedMode < 3)
            {
                pObj.createObjects(room);
                if (pObj.ShowDialog() == DialogResult.OK)
                {
                    if (room.selectedObject.Count == 1)
                    {

                        if (pObj.selectedObject != -1)
                        {
                            byte x = (room.selectedObject[0] as Room_Object).x;
                            byte y = (room.selectedObject[0] as Room_Object).y;
                            room.tilesObjects.Remove((Room_Object)room.selectedObject[0]);
                            room.selectedObject.Clear();
                            Room_Object ro = room.addObject(pObj.selectedObject, (byte)x, (byte)y, 0, (byte)selectedMode);
                            if (ro != null)
                            {

                                ro.get_scroll_x();
                                ro.get_scroll_y();
                                if (ro.special_zero_size != 0)
                                {
                                    ro.size = 1;
                                }

                                ro.setRoom(room);
                                ro.options = ObjectOption.Nothing;
                                room.tilesObjects.Add(ro);
                                room.selectedObject.Add(ro);
                                dragx = 0;
                                dragy = 0;
                                //mouse_down = true;
                                need_refresh = true;
                                room.reloadGfx();
                            }

                        }
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        private void chestpicker_Load(object sender, EventArgs e)
        {

        }

        private void pObj_Load(object sender, EventArgs e)
        {

        }
    }

    public class DScene : DockContent
    {
        public SceneUW scene;
        zscreamForm mainform;
        public string nameText = "";
        public bool namedChanged = false;
        public DScene(zscreamForm mainform,string nameText)
        {
            scene = new SceneUW(mainform);
            this.nameText = nameText;
            this.mainform = mainform;
            GotFocus += DScene_GotFocus;
            
            FormClosing += DScene_FormClosing;
        }

        private void DScene_FormClosing(object sender, FormClosingEventArgs e)
        {


            if (scene.room.has_changed == true)
            {
                //prompt save message
                //e.Cancel = true;
                DialogResult dialogResult = MessageBox.Show("Room has changed. Do you want to save changes?", "Save", MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.Yes) //save
                {
                    scene.room.has_changed = false;
                    mainform.all_rooms[scene.room.index] = (Room)scene.room.Clone();
                    scene.room.clearGFX();
                   
                    mainform.rooms.Remove(this);
                    mainform.loadRoomList(0);
                    mainform.mapPicturebox.Refresh();
                }
                else if (dialogResult == DialogResult.No)
                {
                   
                    scene.room.clearGFX();
                    mainform.rooms.Remove(this);
                    mainform.loadRoomList(0);
                    mainform.mapPicturebox.Refresh();
                }
                else
                {
                    e.Cancel = true;
                }

            }
            else
            {
                scene.room.clearGFX();
                mainform.rooms.Remove(this);
                mainform.loadRoomList(0);
                mainform.mapPicturebox.Refresh();
            }
        }

        private void DScene_GotFocus(object sender, EventArgs e)
        {
            mainform.activeScene = this.scene;
            mainform.mapPropertyGrid.SelectedObject = scene.room;
            scene.room.reloadGfx();
            scene.need_refresh = true;
        }
    }


}
