# Event Registration API – Testing Guide

Single guide for testing the API via **Swagger** or **Postman**. Base URL: `http://localhost:5229` (or the port shown when you run the API). Use **http://localhost:5229/swagger** for Swagger UI.

---

## Requirements Covered

| Requirement | Status |
|-------------|--------|
| Event APIs: Create, Get all, Get by ID, Delete | ✓ |
| Registration APIs: Register (sends email), Get by event, Cancel | ✓ |
| Business rule: Capacity limit | ✓ |
| Business rule: No duplicate email per event | ✓ |
| Business rule: No registration for past events | ✓ |
| Business rule: No delete event when registrations exist | ✓ |
| Email: Confirmation on successful registration (Mailtrap) | ✓ |
| Database schema (Events, Registrations) | ✓ |
| README, Swagger, Postman | ✓ |

---

## 1. Mailtrap: Use Sandbox SMTP (Not API Tokens)

The project uses **Mailtrap Sandbox SMTP**. Do **not** use Settings → API Tokens for sending.

### Where to get credentials

1. In Mailtrap: **Sandboxes** (left menu) → **My Sandbox**.
2. Open **Integration** → **SMTP**.
3. Copy **Host**, **Port**, **Username**, and **Password** from the Credentials section.

### appsettings.Development.json

```json
"Mailtrap": {
  "Host": "sandbox.smtp.mailtrap.io",
  "Port": 587,
  "Username": "PASTE_SMTP_USERNAME_HERE",
  "Password": "PASTE_SMTP_PASSWORD_HERE",
  "SenderEmail": "noreply@eventregistration.local",
  "SenderName": "Event Registration"
}
```

- No API token or permissions needed. Emails are caught in **My Sandbox** inbox (no real delivery).

### Where to see sent emails

**Mailtrap** → **Sandboxes** → **My Sandbox** → open the inbox. Confirmation emails appear there after you register.

---

## 2. Why "Registration for past events is not allowed"

The API blocks registration when the **event date is in the past** (compared to today in UTC).

- Use an event whose **date is in the future** (e.g. 2026-06-15).
- If your event has a past date (e.g. 2025-06-15), registration will always return **400**. Create a new event with a future date or use an existing event with a future date.

---

## 3. Email Not Sent – Check `emailError`

When registration succeeds but the email fails, the API returns **`emailError`** in the response. Use it to debug.

**Example (wrong auth):**
```json
{
  "id": 4,
  "eventId": 4,
  "name": "Test User",
  "email": "test@example.com",
  "registeredAt": "2026-01-31T07:36:20",
  "emailSent": false,
  "emailError": "HttpRequestException: Response status code does not indicate success: 401 (Unauthorized)."
}
```

- **401 Unauthorized** → Switch to **Sandbox SMTP**: use **Sandboxes → My Sandbox → Integration → SMTP** (Username, Password) in `appsettings.Development.json`.
- **InvalidOperationException: Mailtrap SMTP is not configured** → Set **Mailtrap:Host**, **Username**, **Password** from My Sandbox → SMTP.

---

## 4. Example Data for Swagger / Postman

Use **future dates** for events so registration is allowed. Examples below use 2026.

### Create Event (POST /api/Events)

```json
{
  "title": "Developer Conference 2026",
  "description": "Annual developer conference",
  "date": "2026-06-15T09:00:00",
  "capacity": 100,
  "location": "Mumbai Convention Center"
}
```

Small capacity (for testing "capacity reached"):

```json
{
  "title": "Small Workshop",
  "description": "Limited seats",
  "date": "2026-12-01T10:00:00",
  "capacity": 2,
  "location": "Room 101"
}
```

Use the **Id** returned (e.g. `3`) for registration and other calls.

### Register for Event (POST /api/events/{eventId}/registrations)

Use a **future-dated** event Id (e.g. from create above, or existing event with future date).

```json
{
  "name": "Mansi Korat",
  "email": "mansi@example.com"
}
```

Additional users (e.g. for capacity/duplicate tests):

```json
{ "name": "Test User One", "email": "test1@example.com" }
{ "name": "Test User Two", "email": "test2@example.com" }
```

After **201**, check **Mailtrap → My Sandbox** and **Registrations** table in SSMS.

---

## 5. API Endpoints Quick Reference

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/events` | Create event |
| GET | `/api/events` | Get all events |
| GET | `/api/events/{id}` | Get event by ID |
| DELETE | `/api/events/{id}` | Delete event (fails if registrations exist) |
| POST | `/api/events/{eventId}/registrations` | Register for event (sends email) |
| GET | `/api/registrations/{registrationId}` | Get a single registration by ID |
| GET | `/api/events/{eventId}/registrations` | Get registrations for event |
| DELETE | `/api/registrations/{registrationId}` | Cancel registration |

---

## 6. Step-by-Step Testing Scenarios

### Scenario 1: Create Event and Get Events

| Step | Action | Endpoint | Expected |
|------|--------|----------|----------|
| 1.1 | Create event | **POST** `/api/events` | 201 Created, event with Id |
| 1.2 | Get all events | **GET** `/api/events` | 200 OK, list includes new event |
| 1.3 | Get event by ID | **GET** `/api/events/{id}` | 200 OK (use Id from 1.1) |

Use the request body from **§4** (future date).

---

### Scenario 2: Register for Event (Happy Path)

| Step | Action | Endpoint | Expected |
|------|--------|----------|----------|
| 2.1 | Create event (future date) | **POST** `/api/events` | 201 Created |
| 2.2 | Register | **POST** `/api/events/{id}/registrations` | 201 Created, registration with Id |
| 2.3 | Check email | Mailtrap inbox | Email with event name, date, location, registration ID |
| 2.4 | Get registrations | **GET** `/api/events/{id}/registrations` | 200 OK, list includes registration |

---

### Scenario 3: Capacity Not Exceeded

| Step | Action | Endpoint | Expected |
|------|--------|----------|----------|
| 3.1 | Create event with capacity 2 | **POST** `/api/events` (capacity: 2) | 201 Created |
| 3.2 | Register first person | **POST** `/api/events/{id}/registrations` | 201 Created |
| 3.3 | Register second person | **POST** `/api/events/{id}/registrations` | 201 Created |
| 3.4 | Register third person | **POST** `/api/events/{id}/registrations` | 400 – "Event capacity has been reached." |

---

### Scenario 4: Same Email Cannot Register Twice

| Step | Action | Endpoint | Expected |
|------|--------|----------|----------|
| 4.1 | Create event | **POST** `/api/events` | 201 Created |
| 4.2 | Register with `test@example.com` | **POST** `/api/events/{id}/registrations` | 201 Created |
| 4.3 | Register again same email, same event | **POST** `/api/events/{id}/registrations` | 400 – "This email is already registered for this event." |

---

### Scenario 5: No Registration for Past Events

| Step | Action | Endpoint | Expected |
|------|--------|----------|----------|
| 5.1 | Create event with **past date** | **POST** `/api/events` (date in past) | 201 Created |
| 5.2 | Register for that event | **POST** `/api/events/{id}/registrations` | 400 – "Registration for past events is not allowed." |

---

### Scenario 6: Cancel Registration

| Step | Action | Endpoint | Expected |
|------|--------|----------|----------|
| 6.1 | Create event and register | **POST** `/api/events`, then **POST** `/api/events/{id}/registrations` | 201 for both |
| 6.2 | Note **registration Id** from response | — | — |
| 6.3 | Cancel registration | **DELETE** `/api/registrations/{id}` | 204 No Content |
| 6.4 | Get registrations | **GET** `/api/events/{id}/registrations` | 200 OK, list no longer contains that registration |

---

### Scenario 7: Event Cannot Be Deleted If Registrations Exist

| Step | Action | Endpoint | Expected |
|------|--------|----------|----------|
| 7.1 | Create event and register | **POST** `/api/events`, **POST** `/api/events/{id}/registrations` | 201 for both |
| 7.2 | Try to delete event | **DELETE** `/api/events/{id}` | 400 – "Event cannot be deleted because it has existing registrations." |
| 7.3 | Cancel all registrations | **DELETE** `/api/registrations/{id}` for each | 204 No Content |
| 7.4 | Delete event | **DELETE** `/api/events/{id}` | 204 No Content |

---

### Scenario 8: Not Found (404)

| Step | Action | Endpoint | Expected |
|------|--------|----------|----------|
| 8.1 | Get non-existent event | **GET** `/api/events/99999` | 404 Not Found |
| 8.2 | Register for non-existent event | **POST** `/api/events/99999/registrations` | 404 – "Event not found." |
| 8.3 | Cancel non-existent registration | **DELETE** `/api/registrations/99999` | 404 – "Registration not found." |

---

## 7. All Scenarios Summary Table

| # | Scenario | What to do | Expected |
|---|----------|------------|----------|
| 1 | Register – success | Use future-dated event. POST body: `{"name":"Mansi Korat","email":"mansi@example.com"}` | 201, email in Mailtrap, row in `Registrations` |
| 2 | Past event | Register for event with past date | 400 – "Registration for past events is not allowed." |
| 3 | Duplicate email | Register same email twice for same event | First 201, second 400 – "This email is already registered for this event." |
| 4 | Capacity reached | Create event capacity 2, register 2, then 3rd | First two 201, third 400 – "Event capacity has been reached." |
| 5 | Get events | GET /api/events | 200, list of events |
| 6 | Get event by id | GET /api/events/{id} | 200, single event |
| 7 | Get registrations | GET /api/events/{id}/registrations | 200, list of registrations |
| 8 | Cancel registration | DELETE /api/registrations/{id} | 204 |
| 9 | Delete event with registrations | Delete event that has registrations | 400 – "Event cannot be deleted because it has existing registrations." |
| 10 | Delete event after cancel | Cancel all registrations, then DELETE event | 204 |
| 11 | Not found – event | GET /api/events/99999 | 404 |
| 12 | Not found – register | POST /api/events/99999/registrations | 404 – "Event not found." |
| 13 | Not found – cancel | DELETE /api/registrations/99999 | 404 – "Registration not found." |

---

## 8. Quick Checklist

- [ ] Create event (POST `/api/events`)
- [ ] Get all events (GET `/api/events`)
- [ ] Get event by ID (GET `/api/events/{id}`)
- [ ] Register for event and receive Mailtrap email (POST `/api/events/{id}/registrations`)
- [ ] Get registrations by event (GET `/api/events/{id}/registrations`)
- [ ] Cancel registration (DELETE `/api/registrations/{id}`)
- [ ] Capacity exceeded returns 400
- [ ] Duplicate email for same event returns 400
- [ ] Past event registration returns 400
- [ ] Delete event with registrations returns 400; delete after cancelling returns 204
- [ ] Not-found cases return 404

---

## 9. Business Rules – What Is Checked and How to Test

All four business rules are enforced in the API:

| Rule | Where it’s checked | How to test with your data |
|------|--------------------|----------------------------|
| **1. Event capacity cannot be exceeded** | `RegistrationService` – before adding a registration, checks `Registrations.Count >= Capacity` | Create an event with `capacity: 2`, register 2 people, then register a 3rd → **400** "Event capacity has been reached." |
| **2. Same email cannot register twice for the same event** | `RegistrationService` – checks if that email already exists for that event (case-insensitive) | Register `test1@example.com` again for **Event 3** → **400** "This email is already registered for this event." |
| **3. Registration for past events is not allowed** | `RegistrationService` – checks `event.Date < today` (UTC) | Register anyone for **Event 2** (Tech Meetup 2025, date 2025-06-15) → **400** "Registration for past events is not allowed." |
| **4. Event cannot be deleted if registrations exist** | `EventService` + `EventsController` – delete only succeeds when the event has **no** registrations | **DELETE** `/api/events/1` (Marriage has 1 registration) → **400** "Event cannot be deleted because it has existing registrations." **DELETE** `/api/events/2` (no registrations) → **204** success. |

**Rule 4 – “Which user do I delete?”**

- The rule is about **deleting the event**, not the user. You don’t delete a “user” to check the rule.
- **To see the block:** Try to **delete an event** that still has registrations (e.g. **DELETE** `/api/events/1`) → you get **400**.
- **To be able to delete the event:** First **cancel** all registrations for that event (e.g. **DELETE** `/api/registrations/1` for Mansi’s registration on Event 1). Then **DELETE** `/api/events/1` → **204**.

So: to test “event cannot be deleted if registrations exist,” try deleting **Event 1** (or 3, 4, 5) and expect 400. To test “event can be deleted after registrations are gone,” cancel **Registration Id 1** (Mansi for Event 1), then delete **Event 1**.

---

## 10. Postman

1. Run the API.
2. In Postman: **Import → Link** and paste: `http://localhost:5229/swagger/v1/swagger.json` (use your actual port if different).
