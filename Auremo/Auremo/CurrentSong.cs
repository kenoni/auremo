﻿/*
 * Copyright 2013 Mikko Teräs and Niilo Säämänen.
 *
 * This file is part of Auremo.
 *
 * Auremo is free software: you can redistribute it and/or modify it under the
 * terms of the GNU General Public License as published by the Free Software
 * Foundation, version 2.
 *
 * Auremo is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR
 * A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with Auremo. If not, see http://www.gnu.org/licenses/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Auremo
{
    public class CurrentSong : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        private DataModel m_DataModel = null;
        private string m_DisplayString = "";
        private Playable m_CurrentPlayable = null;

        public CurrentSong(DataModel dataModel)
        {
            m_DataModel = dataModel;
            m_DataModel.ServerStatus.PropertyChanged += new PropertyChangedEventHandler(OnServerStatusPropertyChanged);
            Update();
            BuildDisplayString();
        }

        public void Update()
        {
            m_DataModel.ServerSession.CurrentSong();
        }

        public void OnCurrentSongResponseReceived(IEnumerable<MPDSongResponseBlock> response)
        {
            m_CurrentPlayable = response.First().ToPlayable(m_DataModel.Database.DateNormalizer);
            BuildDisplayString();
        }

        public string DisplayString
        {
            get
            {
                return m_DisplayString;
            }
            private set
            {
                if (value != m_DisplayString)
                {
                    m_DisplayString = value;
                    NotifyPropertyChanged("DisplayString");
                }
            }
        }

        private void OnServerStatusPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentSongIndex" || e.PropertyName == "PlaylistVersion")
            {
                Update();
            }
            else if (e.PropertyName == "State")
            {
                BuildDisplayString();
            }
        }

        private void BuildDisplayString()
        {
            if (m_DataModel.ServerStatus.IsPlaying)
            {
                if (m_CurrentPlayable == null)
                {
                    DisplayString = "Playing.";
                }
                else
                {
                    DisplayString = "Playing " + CurrentPlayableToString() + ".";
                }
            }
            else if (m_DataModel.ServerStatus.IsPaused)
            {
                if (m_CurrentPlayable == null)
                {
                    DisplayString = "Paused.";
                }
                else
                {
                    DisplayString = "Paused " + CurrentPlayableToString() + ".";
                }
            }
            else if (m_DataModel.ServerStatus.IsStopped)
            {
                DisplayString = "Stopped.";
            }
        }

        private string CurrentPlayableToString()
        {
            if (m_CurrentPlayable is SongMetadata)
            {
                return CurrentSongToString();
            }
            else if (m_CurrentPlayable is StreamMetadata)
            {
                return CurrentStreamToString();
            }
            else
            {
                return m_CurrentPlayable.Path;
            }
        }

        private string CurrentSongToString()
        {
            SongMetadata song = m_CurrentPlayable as SongMetadata;
            StringBuilder result = new StringBuilder(); 

            result.Append(song.Artist);
            result.Append(": ");
            result.Append(song.Title);
            result.Append(" (");
            result.Append(song.Album);

            if (song.Year != null)
            {
                result.Append(", ");
                result.Append(song.Year);
            }

            result.Append(")");

            return result.ToString();
        }

        private string CurrentStreamToString()
        {
            StreamMetadata stream = m_CurrentPlayable as StreamMetadata;
            StringBuilder result = new StringBuilder();

            if (stream.Name == null)
            {
                result.Append(stream.Path);
            }
            else
            {
                result.Append(stream.Title);
                result.Append(" (");
                result.Append(stream.Path);
                result.Append(")");
            }

            return result.ToString();
        }
    }
}
