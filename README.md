# RaceDay

## Project Description

RaceDay is a RESTful API built with ASP.NET Core Web API and Entity Framework Core for managing running, walking, and cycling events. The API supports two user roles, secure authentication with hashed passwords, event and category management, participant enrolment, and result capture.

This repository contains **Part 2** of the PROG6212 PoE — the RESTful API that powers the RaceDay system.

---

## User Roles

### Organiser

- Register and log in securely
- Create, update, and delete their own events
- Create, update, and delete categories for their own events
- View all enrolments for their own events
- Update enrolment statuses (Pending / Confirmed / Cancelled)
- Capture results (finish time and position)

### Participant

- Register and log in securely
- View all events and their categories
- Enrol in an event by choosing a category
- View their own enrolments
- View their own results after an event

---

## Part 1

Part 1 consisted of the system analysis, ERD design, and endpoint planning for the RaceDay system. The database schema and API endpoints in this part are built to match that plan.

---

## Part 2

Part 2 is the implementation of the RaceDay RESTful API. It includes authentication, role-based access control, event and category management, participant enrolment, and result capture — all documented in Swagger and covered by unit tests.

### Technologies Used

- ASP.NET Core Web API (.NET 10)
- C#
- Microsoft SQL Server (Express)
- Entity Framework Core (Code-First)
- Session-based authentication with BCrypt password hashing
- Swagger / OpenAPI with XML comments
- xUnit with in-memory database
- GitHub Actions

###ci Screenshot
<img width="916" height="434" alt="prog screnshot readme" src="https://github.com/user-attachments/assets/997f49e3-9bae-42ba-ae8e-daf4c0adac3a" />

### youtube video
https://youtu.be/rb6EcvhhOjg
