# Basketball Play Creator (Unity + FastAPI)

This project is a simple mobile app built with **Unity** that allows users to create, view, and share basketball plays.  
Players can be dragged around the court to design plays, teams can be shared through invitation codes, and all data is synced with a **FastAPI backend**.

## Features
- Create and manage teams
- Share teams using invitation codes
- Drag-and-drop play creator
- Save and edit basketball plays
- User roles (owner, admin, viewer)
- Connects to a FastAPI backend for:
  - Authentication
  - Team management
  - Play storage
  - Invitation code handling

## Technology
- Unity 2021+
- C# (coroutines, UI, TextMeshPro)
- FastAPI backend (Python)
- REST communication between Unity and API

## Setup (Unity)
1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/basketball-play-creator.git
