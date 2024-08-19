YouTube Downloader - C# WinForms Application
Project Overview
This C# WinForms application, named YouTube Downloader, is designed for downloading individual videos or entire playlists from YouTube. The application allows users to download videos in the highest available quality, providing a user-friendly interface for managing downloads. Additionally, the application features a download speed monitor, giving users real-time feedback on their internet speed during the download process.

Features
Download YouTube Videos: Users can input a YouTube video URL and download the video in the highest available quality.
Download YouTube Playlists: The application supports downloading entire playlists, saving each video to a user-specified directory.
Download Speed Monitor: Displays the current download speed in Mbps, updating every second.
Clipboard Integration: Users can quickly paste video or playlist URLs from their clipboard into the application.
Progress Feedback: A progress bar provides visual feedback on the download process.
Technologies Used
C# .NET: The application is built using C# and the .NET WinForms framework.
YoutubeExplode: A library used for interacting with YouTube to fetch video and playlist information and download streams.
AngleSharp.Io: Used for handling input and output operations.
System.Text.RegularExpressions: For sanitizing video titles to be used as file names.
System.Net.NetworkInformation: Used to monitor network activity and calculate download speed.
