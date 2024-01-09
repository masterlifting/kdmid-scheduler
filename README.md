<!-- @format -->

# kdmid-scheduler

## Overview

kdmid-scheduler showcases my full-stack development skills and expertise in crafting high-quality code. This application is designed to facilitate appointments with Russian embassies worldwide, demonstrating a well-structured, patterned codebase integrated with my own infrastructure libraries.

## Technologies

The backend is built using **.NET 8**, **Docker**, **MongoDB**, **Telegram.Bot**, and **Azure**, while the frontend features **React**, **Redux ToolKit**, and **Tailwind CSS**. This application is flexible enough to be hosted on Azure or any VPS.

## Repository Structure

This repository adopts a git modules approach. To clone the entire application code, use the command:

```bash
git clone https://github.com/masterlifting/kdmid-scheduler.git --recurse-submodules
```

## Setup and Configuration

Note that the docker-compose might not run correctly due to specific environment variables and a need for manual configuration of a portion of the MongoDB cluster. However, you can explore the codebase for a comprehensive understanding.

## User Interface

The user interface is presented through the Telegram bot ([kdmid-scheduler_bot](https://t.me/kdmidscheduler_bot)). The `src` directory contains `backend` and `frontend` folders along with a docker-compose file for VPS production deployment.

### Backend

The `backend` folder includes:

- Core libraries for the application.
- Hosts of executable apps dependent on the host type.
- `core` subfolder with projects divided into layers resembling clean architecture and submodules.
- `submodules` containing my custom infrastructure libraries, usable as Nuget packages.

### Frontend

The `frontend` folder houses the Telegram web app, developed using React, designed for the Telegram bot.

---

This application is a testament to my capabilities as a full-stack software engineer, showcasing a thorough understanding of modern development practices and technologies.
