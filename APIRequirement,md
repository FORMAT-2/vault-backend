# Vault R2 — API Documentation

Complete reference for all REST endpoints and real-time WebSocket (SignalR) events used by the Vault R2 application.

---

## Table of Contents

1. [Base URL & Environment](#1-base-url--environment)
2. [Authentication](#2-authentication)
3. [Shared Type Schemas](#3-shared-type-schemas)
4. [Auth & User Management](#4-auth--user-management)
   - 4.1 [Login](#41-login)
   - 4.2 [Register](#42-register)
   - 4.3 [Update Profile](#43-update-profile)
   - 4.4 [Search Users](#44-search-users)
   - 4.5 [Request OTP](#45-request-otp)
   - 4.6 [Verify OTP](#46-verify-otp)
   - 4.7 [Reset Password](#47-reset-password)
5. [Media (Photos & Videos)](#5-media-photos--videos)
   - 5.1 [Get All Media](#51-get-all-media)
   - 5.2 [Upload Media](#52-upload-media)
   - 5.3 [Add Comment](#53-add-comment)
   - 5.4 [Like Media](#54-like-media)
6. [Social & Friends](#6-social--friends)
   - 6.1 [Send Friend Request](#61-send-friend-request)
   - 6.2 [Get Friend Requests](#62-get-friend-requests)
   - 6.3 [Respond to Friend Request](#63-respond-to-friend-request)
   - 6.4 [Get Friends List](#64-get-friends-list)
7. [Chat & Messaging](#7-chat--messaging)
   - 7.1 [Get Messages](#71-get-messages)
   - 7.2 [Send Message (REST Fallback)](#72-send-message-rest-fallback)
8. [Emergency & Safety](#8-emergency--safety)
   - 8.1 [Get Emergency Settings](#81-get-emergency-settings)
   - 8.2 [Save Emergency Settings](#82-save-emergency-settings)
   - 8.3 [Trigger SOS](#83-trigger-sos)
9. [Real-Time: SignalR Hub](#9-real-time-signalr-hub)
   - 9.1 [Connection](#91-connection)
   - 9.2 [Chat Messages](#92-chat-messages)
   - 9.3 [Voice & Video Calls (WebRTC Signaling)](#93-voice--video-calls-webrtc-signaling)
10. [Partner Management](#10-partner-management)
    - 10.1 [Set Partner](#101-set-partner)
    - 10.2 [Get Partner](#102-get-partner)
    - 10.3 [Remove Partner](#103-remove-partner)
11. [Location Sharing](#11-location-sharing)
    - 11.1 [Update My Location](#111-update-my-location)
    - 11.2 [Get Partner Location](#112-get-partner-location)
12. [Real-Time: Additional SignalR Events](#12-real-time-additional-signalr-events)
    - 12.1 [Miss You Signal (Vibration)](#121-miss-you-signal-vibration)
    - 12.2 [Location Updates (Real-time)](#122-location-updates-real-time)
    - 12.3 [SOS Alert (Real-time)](#123-sos-alert-real-time)
13. [Emergency SOS — WhatsApp Integration](#13-emergency-sos--whatsapp-integration)
    - 13.1 [SOS Flow](#131-sos-flow)
    - 13.2 [Trigger SOS (Enhanced)](#132-trigger-sos-enhanced)
14. [Error Responses](#14-error-responses)

---

## 1. Base URL & Environment

| Environment | Base URL |
|---|---|
| Local development | `http://localhost:5116/api` |
| SignalR Hub | `http://localhost:5116/ws` |

All REST paths below are relative to the base URL (e.g. `POST /auth/login` → `POST http://localhost:5116/api/auth/login`).

---

## 2. Authentication

All endpoints **except** `POST /auth/login` and `POST /auth/register` require a JWT bearer token.

```
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json
```

The token is returned by the login and register endpoints and must be stored by the client (the app stores it in `localStorage` under the key `auth_token`).

---

## 3. Shared Type Schemas

These TypeScript interfaces describe the data shapes used throughout the API.

```typescript
interface User {
  id: string;
  username: string;
  email: string;
  avatar?: string;           // URL to avatar image
}

interface Comment {
  id: string;
  userId: string;
  username: string;
  text: string;
  createdAt: string;         // ISO 8601
}

interface Photo {
  id: string;
  userId: string;
  username: string;
  url: string;               // URL to stored media
  type: "image" | "video";
  caption: string;
  comments: Comment[];
  createdAt: string;         // ISO 8601
  likes: number;
}

interface DirectMessage {
  id: string;
  senderId: string;
  senderName: string;
  receiverId: string;
  text: string;
  timestamp: string;         // ISO 8601
  type?: "text" | "location";
  location?: {
    lat: number;
    lng: number;
  };
}

interface FriendRequest {
  id: string;
  fromUserId: string;
  fromUsername: string;
  toUserId: string;
  status: "pending" | "accepted" | "rejected";
  createdAt: string;         // ISO 8601
}

interface EmergencyContact {
  id: string;
  name: string;
  phone: string;
}

interface EmergencySettings {
  contacts: EmergencyContact[];
  customMessage: string;
}
```

---

## 4. Auth & User Management

### 4.1 Login

Authenticate an existing user and receive a JWT token.

- **Method:** `POST`
- **Path:** `/auth/login`
- **Auth required:** No

**Request Body**

```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `email` | string | ✅ | Registered email address |
| `password` | string | ✅ | Account password (plain text over HTTPS) |

**Response — `200 OK`**

```json
{
  "user": {
    "id": "64b1f2c3d4e5f6a7b8c9d0e1",
    "username": "johndoe",
    "email": "user@example.com",
    "avatar": "https://cdn.example.com/avatars/johndoe.png"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

---

### 4.2 Register

Create a new account.

- **Method:** `POST`
- **Path:** `/auth/register`
- **Auth required:** No

**Request Body**

```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "password123",
  "avatar": "https://cdn.example.com/avatars/johndoe.png"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `username` | string | ✅ | Display name |
| `email` | string | ✅ | Must be unique |
| `password` | string | ✅ | Minimum 6 characters recommended |
| `avatar` | string \| null | ❌ | URL to avatar image |

**Response — `200 OK`**

```json
{
  "user": {
    "id": "64b1f2c3d4e5f6a7b8c9d0e1",
    "username": "johndoe",
    "email": "john@example.com",
    "avatar": "https://cdn.example.com/avatars/johndoe.png"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

---

### 4.3 Update Profile

Update the authenticated user's display name and/or avatar.

- **Method:** `PUT`
- **Path:** `/users/profile`
- **Auth required:** Yes

**Request Body**

```json
{
  "userId": "64b1f2c3d4e5f6a7b8c9d0e1",
  "username": "newname",
  "avatar": "https://cdn.example.com/avatars/newname.png"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `userId` | string | ✅ | ID of the user to update (must match the authenticated user) |
| `username` | string | ✅ | New display name |
| `avatar` | string \| null | ❌ | New avatar URL; pass `null` to clear |

**Response — `200 OK`**

```json
{
  "id": "64b1f2c3d4e5f6a7b8c9d0e1",
  "username": "newname",
  "email": "john@example.com",
  "avatar": "https://cdn.example.com/avatars/newname.png"
}
```

---

### 4.4 Search Users

Search for users by username or email.

- **Method:** `GET`
- **Path:** `/users/search?q={query}`
- **Auth required:** Yes

**Query Parameters**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `q` | string | ✅ | Search term matched against username or email |

**Response — `200 OK`**

```json
[
  {
    "id": "64b1f2c3d4e5f6a7b8c9d0e1",
    "username": "johndoe",
    "email": "john@example.com",
    "avatar": "https://cdn.example.com/avatars/johndoe.png"
  }
]
```

Returns an empty array `[]` when no matches are found.

---

### 4.5 Request OTP

Send a one-time password to the given email address (first step of password reset).

- **Method:** `POST`
- **Path:** `/auth/otp/request`
- **Auth required:** No

**Request Body**

```json
{
  "email": "john@example.com"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `email` | string | ✅ | Email address of the account to reset |

**Response — `200 OK`**

```json
{
  "otp": "834712"
}
```

> **Note:** In production the OTP is delivered via email/SMS and the `otp` field should **not** be returned in the response. It is exposed here for development convenience only.

---

### 4.6 Verify OTP

Confirm that a received OTP is valid (second step of password reset).

- **Method:** `POST`
- **Path:** `/auth/otp/verify`
- **Auth required:** No

**Request Body**

```json
{
  "email": "john@example.com",
  "otp": "834712"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `email` | string | ✅ | Email address associated with the OTP |
| `otp` | string | ✅ | The one-time password received |

**Response — `200 OK`**

```json
{
  "success": true
}
```

---

### 4.7 Reset Password

Set a new password after OTP verification (third step of password reset).

- **Method:** `POST`
- **Path:** `/auth/password/reset`
- **Auth required:** No

**Request Body**

```json
{
  "email": "john@example.com",
  "newPassword": "newpassword123"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `email` | string | ✅ | Email address of the account |
| `newPassword` | string | ✅ | The new password to set |

**Response — `200 OK`** _(empty body)_

---

## 5. Media (Photos & Videos)

### 5.1 Get All Media

Retrieve all media items (photos and videos) for the authenticated vault.

- **Method:** `GET`
- **Path:** `/media`
- **Auth required:** Yes

**Response — `200 OK`**

```json
[
  {
    "id": "media_abc123",
    "userId": "64b1f2c3d4e5f6a7b8c9d0e1",
    "username": "johndoe",
    "url": "https://cdn.example.com/media/photo1.jpg",
    "type": "image",
    "caption": "Sunset at the beach",
    "comments": [
      {
        "id": "comment_xyz",
        "userId": "64b1f2c3d4e5f6a7b8c9d0e1",
        "username": "johndoe",
        "text": "Beautiful!",
        "createdAt": "2024-01-15T10:30:00.000Z"
      }
    ],
    "createdAt": "2024-01-15T09:00:00.000Z",
    "likes": 3
  }
]
```

---

### 5.2 Upload Media

Upload a photo or video. The file is encoded as a Base64 data URL.

- **Method:** `POST`
- **Path:** `/media/upload`
- **Auth required:** Yes

**Request Body**

```json
{
  "userId": "64b1f2c3d4e5f6a7b8c9d0e1",
  "username": "johndoe",
  "mediaData": "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAA...",
  "type": "image",
  "caption": "Sunset at the beach"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `userId` | string | ✅ | ID of the uploading user |
| `username` | string | ✅ | Display name of the uploading user |
| `mediaData` | string | ✅ | Base64-encoded data URL including MIME prefix (`data:image/jpeg;base64,...` or `data:video/mp4;base64,...`) |
| `type` | `"image"` \| `"video"` | ✅ | Media type |
| `caption` | string | ✅ | Caption text (may be empty string) |

**Response — `200 OK`**

```json
{
  "id": "media_abc123",
  "userId": "64b1f2c3d4e5f6a7b8c9d0e1",
  "username": "johndoe",
  "url": "https://cdn.example.com/media/photo1.jpg",
  "type": "image",
  "caption": "Sunset at the beach",
  "comments": [],
  "createdAt": "2024-01-15T09:00:00.000Z",
  "likes": 0
}
```

---

### 5.3 Add Comment

Add a comment to a media item.

- **Method:** `POST`
- **Path:** `/media/comment`
- **Auth required:** Yes

**Request Body**

```json
{
  "photoId": "media_abc123",
  "userId": "64b1f2c3d4e5f6a7b8c9d0e1",
  "text": "Amazing shot!"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `photoId` | string | ✅ | ID of the media item being commented on |
| `userId` | string | ✅ | ID of the commenting user |
| `text` | string | ✅ | Comment content |

**Response — `200 OK`**

```json
{
  "id": "comment_xyz",
  "userId": "64b1f2c3d4e5f6a7b8c9d0e1",
  "username": "johndoe",
  "text": "Amazing shot!",
  "createdAt": "2024-01-15T10:30:00.000Z"
}
```

---

### 5.4 Like Media

Toggle a like on a media item.

- **Method:** `POST`
- **Path:** `/media/like`
- **Auth required:** Yes

**Request Body**

```json
{
  "photoId": "media_abc123",
  "userId": "64b1f2c3d4e5f6a7b8c9d0e1"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `photoId` | string | ✅ | ID of the media item |
| `userId` | string | ✅ | ID of the user liking the item |

**Response — `200 OK`** _(empty body)_

---

## 6. Social & Friends

### 6.1 Send Friend Request

Send a friend request to another user.

- **Method:** `POST`
- **Path:** `/social/request/send`
- **Auth required:** Yes

**Request Body**

```json
{
  "toUserId": "64b1f2c3d4e5f6a7b8c9d0e2"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `toUserId` | string | ✅ | ID of the user to send the request to |

> The `fromUserId` is derived from the authenticated user's JWT token on the server.

**Response — `200 OK`** _(empty body)_

---

### 6.2 Get Friend Requests

Retrieve all incoming (pending) friend requests for the authenticated user.

- **Method:** `GET`
- **Path:** `/social/requests`
- **Auth required:** Yes

**Response — `200 OK`**

```json
[
  {
    "id": "req_123",
    "fromUserId": "64b1f2c3d4e5f6a7b8c9d0e2",
    "fromUsername": "janedoe",
    "toUserId": "64b1f2c3d4e5f6a7b8c9d0e1",
    "status": "pending",
    "createdAt": "2024-01-14T08:00:00.000Z"
  }
]
```

Returns an empty array `[]` when there are no pending requests.

---

### 6.3 Respond to Friend Request

Accept or reject an incoming friend request.

- **Method:** `POST`
- **Path:** `/social/request/respond`
- **Auth required:** Yes

**Request Body**

```json
{
  "requestId": "req_123",
  "status": "accepted"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `requestId` | string | ✅ | ID of the friend request |
| `status` | `"accepted"` \| `"rejected"` | ✅ | The decision |

**Response — `200 OK`** _(empty body)_

---

### 6.4 Get Friends List

Retrieve all accepted friends for the authenticated user.

- **Method:** `GET`
- **Path:** `/social/friends`
- **Auth required:** Yes

**Response — `200 OK`**

```json
[
  {
    "id": "64b1f2c3d4e5f6a7b8c9d0e2",
    "username": "janedoe",
    "email": "jane@example.com",
    "avatar": "https://cdn.example.com/avatars/janedoe.png"
  }
]
```

Returns an empty array `[]` when the user has no friends yet.

---

## 7. Chat & Messaging

The primary real-time chat path uses the SignalR hub (see [Section 9](#9-real-time-signalr-hub)). The REST endpoints below serve as the **persistence layer**: fetching message history and acting as a fallback when the WebSocket is unavailable.

### 7.1 Get Messages

Fetch the message history between the authenticated user and a specific friend.

- **Method:** `GET`
- **Path:** `/chat/messages/{friendId}`
- **Auth required:** Yes

**Path Parameters**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `friendId` | string | ✅ | ID of the other participant in the conversation |

**Response — `200 OK`**

```json
[
  {
    "id": "msg_001",
    "senderId": "64b1f2c3d4e5f6a7b8c9d0e1",
    "senderName": "johndoe",
    "receiverId": "64b1f2c3d4e5f6a7b8c9d0e2",
    "text": "Hey, how are you?",
    "timestamp": "2024-01-15T11:00:00.000Z",
    "type": "text"
  },
  {
    "id": "msg_002",
    "senderId": "64b1f2c3d4e5f6a7b8c9d0e1",
    "senderName": "johndoe",
    "receiverId": "64b1f2c3d4e5f6a7b8c9d0e2",
    "text": "📍 Live Location: 51.50740, -0.12780",
    "timestamp": "2024-01-15T11:05:00.000Z",
    "type": "location",
    "location": {
      "lat": 51.5074,
      "lng": -0.1278
    }
  }
]
```

Messages are returned in ascending chronological order. Returns an empty array `[]` when there is no history.

---

### 7.2 Send Message (REST Fallback)

Persist a message to the database. Used as a fallback when the SignalR connection is unavailable.

- **Method:** `POST`
- **Path:** `/chat/send`
- **Auth required:** Yes

**Request Body — Text Message**

```json
{
  "id": "msg_003",
  "senderId": "64b1f2c3d4e5f6a7b8c9d0e1",
  "senderName": "johndoe",
  "receiverId": "64b1f2c3d4e5f6a7b8c9d0e2",
  "text": "Hello!",
  "timestamp": "2024-01-15T11:10:00.000Z",
  "type": "text"
}
```

**Request Body — Location Message**

```json
{
  "id": "msg_004",
  "senderId": "64b1f2c3d4e5f6a7b8c9d0e1",
  "senderName": "johndoe",
  "receiverId": "64b1f2c3d4e5f6a7b8c9d0e2",
  "text": "📍 Live Location: 51.50740, -0.12780",
  "timestamp": "2024-01-15T11:15:00.000Z",
  "type": "location",
  "location": {
    "lat": 51.5074,
    "lng": -0.1278
  }
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | string | ✅ | Client-generated UUID |
| `senderId` | string | ✅ | ID of the sender |
| `senderName` | string | ✅ | Display name of the sender |
| `receiverId` | string | ✅ | ID of the recipient |
| `text` | string | ✅ | Message text content |
| `timestamp` | string | ✅ | ISO 8601 creation time |
| `type` | `"text"` \| `"location"` | ❌ | Defaults to `"text"` when omitted |
| `location` | `{ lat: number, lng: number }` | ❌ | Required when `type` is `"location"` |

**Response — `200 OK`** _(empty body)_

---

## 8. Emergency & Safety

### 8.1 Get Emergency Settings

Retrieve the authenticated user's emergency contacts and SOS message template.

- **Method:** `GET`
- **Path:** `/safety/settings`
- **Auth required:** Yes

**Response — `200 OK`**

```json
{
  "contacts": [
    {
      "id": "ec_001",
      "name": "Jane Doe",
      "phone": "+1-555-000-1234"
    },
    {
      "id": "ec_002",
      "name": "Bob Smith",
      "phone": "+1-555-000-5678"
    }
  ],
  "customMessage": "I need help. My current location is attached."
}
```

Returns a settings object with empty `contacts` array and a default message when no settings have been saved.

---

### 8.2 Save Emergency Settings

Create or replace the emergency settings for the authenticated user.

- **Method:** `POST`
- **Path:** `/safety/settings`
- **Auth required:** Yes

**Request Body**

```json
{
  "contacts": [
    {
      "id": "ec_001",
      "name": "Jane Doe",
      "phone": "+1-555-000-1234"
    }
  ],
  "customMessage": "SOS! Please send help to my location."
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `contacts` | `EmergencyContact[]` | ✅ | Up to 5 emergency contacts |
| `contacts[].id` | string | ✅ | Client-generated ID |
| `contacts[].name` | string | ✅ | Contact's full name |
| `contacts[].phone` | string | ✅ | Contact's phone number |
| `customMessage` | string | ✅ | Message body sent with every SOS alert |

**Response — `200 OK`** _(empty body)_

---

### 8.3 Trigger SOS

Dispatch an emergency alert to all saved contacts using the stored SOS message.

- **Method:** `POST`
- **Path:** `/safety/trigger`
- **Auth required:** Yes

**Request Body**

```json
{
  "userId": "64b1f2c3d4e5f6a7b8c9d0e1",
  "location": {
    "lat": 51.5074,
    "lng": -0.1278
  }
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `userId` | string | ✅ | ID of the user triggering the SOS |
| `location` | `{ lat: number, lng: number }` | ✅ | Current GPS coordinates to include in the alert |

**Response — `200 OK`** _(empty body)_

The server looks up the user's saved contacts and `customMessage`, then delivers alerts via SMS/push notification (e.g. AWS SNS) to each contact.

---

## 9. Real-Time: SignalR Hub

The application uses **ASP.NET Core SignalR** for all real-time features (chat messages, call signaling). The client library used is `@microsoft/signalr`.

### 9.1 Connection

**Hub URL:** `http://localhost:5116/ws`

**Authentication:** The JWT token is supplied via the `accessTokenFactory` option:

```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5116/ws", {
    accessTokenFactory: () => localStorage.getItem("auth_token") || "",
    transport: signalR.HttpTransportType.All
  })
  .withAutomaticReconnect()
  .build();

await connection.start();
```

The hub supports WebSockets, Server-Sent Events, and Long Polling as transports (negotiated automatically).

---

### 9.2 Chat Messages

#### Client → Server: `SendMessage`

Send a direct message to another user. The server must broadcast it to the recipient.

```typescript
// Invoke from client
connection.invoke("SendMessage", receiverId: string, message: DirectMessage);
```

**`message` payload**

```json
{
  "id": "msg_003",
  "senderId": "64b1f2c3d4e5f6a7b8c9d0e1",
  "senderName": "johndoe",
  "receiverId": "64b1f2c3d4e5f6a7b8c9d0e2",
  "text": "Hey!",
  "timestamp": "2024-01-15T11:10:00.000Z",
  "type": "text"
}
```

For a **location message**, include the additional fields:

```json
{
  "id": "msg_004",
  "senderId": "64b1f2c3d4e5f6a7b8c9d0e1",
  "senderName": "johndoe",
  "receiverId": "64b1f2c3d4e5f6a7b8c9d0e2",
  "text": "📍 Live Location: 51.50740, -0.12780",
  "timestamp": "2024-01-15T11:15:00.000Z",
  "type": "location",
  "location": {
    "lat": 51.5074,
    "lng": -0.1278
  }
}
```

#### Server → Client: `ReceiveMessage`

The server pushes incoming messages to the recipient.

```typescript
connection.on("ReceiveMessage", (message: DirectMessage) => {
  // add message to chat UI
});
```

The `message` object has the same shape as the `SendMessage` payload above.

---

### 9.3 Voice & Video Calls (WebRTC Signaling)

The hub acts as a signaling relay for WebRTC peer connections. All call events carry a `to` field (the target user's ID) so the server can route the payload to the correct recipient.

#### Step-by-step call flow

```
Caller                     Server (Hub)               Callee
  |                              |                        |
  |-- INITIATE_CALL -----------> |                        |
  |   { offer, callType, to,     |                        |
  |     from }                   |                        |
  |                              |-- INCOMING_CALL ------> |
  |                              |   { offer, callType,   |
  |                              |     fromId, from }     |
  |                              |                        |
  |                              | <-- ANSWER_CALL -------|
  |                              |    { answer, to }      |
  | <-- CALL_ANSWER -------------|                        |
  |    { answer }                |                        |
  |                              |                        |
  |-- SEND_ICE_CANDIDATE ------> |                        |
  |   { candidate, to }          |-- ICE_CANDIDATE -----> |
  |                              |   { candidate }        |
  |                              |                        |
  | <-- ICE_CANDIDATE -----------|                        |
  |   { candidate }              | <-- SEND_ICE_CANDIDATE-|
  |                              |    { candidate, to }   |
  |                              |                        |
  |-- END_CALL ----------------> |                        |
  |   { to }                     |-- END_CALL ----------> |
```

---

#### Client → Server: `INITIATE_CALL`

Sent by the caller to begin a call session.

```typescript
connection.invoke("INITIATE_CALL", {
  offer: RTCSessionDescriptionInit,  // WebRTC SDP offer
  callType: "audio" | "video",
  to: string,        // receiverId
  from: string       // caller's username
});
```

**Payload**

```json
{
  "offer": {
    "type": "offer",
    "sdp": "v=0\r\no=- 46117..."
  },
  "callType": "video",
  "to": "64b1f2c3d4e5f6a7b8c9d0e2",
  "from": "johndoe"
}
```

#### Server → Client: `INCOMING_CALL`

Pushed to the callee when a new call arrives.

```typescript
connection.on("INCOMING_CALL", (data: {
  offer: RTCSessionDescriptionInit;
  callType: "audio" | "video";
  fromId: string;
  from: string;
}) => {
  // show incoming call UI
});
```

**Payload**

```json
{
  "offer": {
    "type": "offer",
    "sdp": "v=0\r\no=- 46117..."
  },
  "callType": "video",
  "fromId": "64b1f2c3d4e5f6a7b8c9d0e1",
  "from": "johndoe"
}
```

---

#### Client → Server: `ANSWER_CALL`

Sent by the callee after accepting the call.

```typescript
connection.invoke("ANSWER_CALL", {
  answer: RTCSessionDescriptionInit,
  to: string  // callerId
});
```

**Payload**

```json
{
  "answer": {
    "type": "answer",
    "sdp": "v=0\r\no=- 98234..."
  },
  "to": "64b1f2c3d4e5f6a7b8c9d0e1"
}
```

#### Server → Client: `CALL_ANSWER`

Pushed to the caller once the callee accepts.

```typescript
connection.on("CALL_ANSWER", (data: {
  answer: RTCSessionDescriptionInit;
}) => {
  // set remote description on PeerConnection
});
```

**Payload**

```json
{
  "answer": {
    "type": "answer",
    "sdp": "v=0\r\no=- 98234..."
  }
}
```

---

#### Client → Server: `SEND_ICE_CANDIDATE`

Exchange ICE candidates during connection establishment.

```typescript
connection.invoke("SEND_ICE_CANDIDATE", {
  candidate: RTCIceCandidateInit,
  to: string
});
```

**Payload**

```json
{
  "candidate": {
    "candidate": "candidate:842163049 1 udp 1677729535 192.168.1.5 46154 typ srflx...",
    "sdpMid": "0",
    "sdpMLineIndex": 0
  },
  "to": "64b1f2c3d4e5f6a7b8c9d0e2"
}
```

#### Server → Client: `ICE_CANDIDATE`

Pushed to the peer with a new ICE candidate.

```typescript
connection.on("ICE_CANDIDATE", (data: {
  candidate: RTCIceCandidateInit;
}) => {
  peerConnection.addIceCandidate(new RTCIceCandidate(data.candidate));
});
```

**Payload**

```json
{
  "candidate": {
    "candidate": "candidate:842163049 1 udp 1677729535 192.168.1.5 46154 typ srflx...",
    "sdpMid": "0",
    "sdpMLineIndex": 0
  }
}
```

---

#### Client → Server: `END_CALL`

Sent by either participant to terminate the call.

```typescript
connection.invoke("END_CALL", {
  to: string  // the other participant's ID
});
```

**Payload**

```json
{
  "to": "64b1f2c3d4e5f6a7b8c9d0e2"
}
```

#### Server → Client: `END_CALL`

Pushed to the other participant to tear down the WebRTC connection.

```typescript
connection.on("END_CALL", () => {
  // close PeerConnection, stop media tracks
});
```

---

## 10. Partner Management

### 10.1 Set Partner

Set a friend as your partner for live location sharing and mutual tracking.

- **Method:** `POST`
- **Path:** `/social/partner/set`
- **Auth required:** Yes

**Request Body**

```json
{
  "partnerId": "friendUserId"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `partnerId` | string | ✅ | A userId from the authenticated user's friends list |

**Response — `200 OK`** _(empty body)_

---

### 10.2 Get Partner

Get the current partner for the authenticated user.

- **Method:** `GET`
- **Path:** `/social/partner`
- **Auth required:** Yes

**Response — `200 OK`**

```json
{
  "partnerId": "friendUserId"
}
```

Returns `{ "partnerId": null }` when no partner is set.

---

### 10.3 Remove Partner

Remove the current partner association.

- **Method:** `DELETE`
- **Path:** `/social/partner/remove`
- **Auth required:** Yes

**Response — `200 OK`** _(empty body)_

---

## 11. Location Sharing

### 11.1 Update My Location

Publish the authenticated user's live location (used by the socket for real-time and REST as a fallback).

- **Method:** `POST`
- **Path:** `/api/location/update`
- **Auth required:** Yes

**Request Body**

```json
{
  "lat": 51.5074,
  "lng": -0.1278,
  "accuracy": 10.5,
  "timestamp": "2024-01-15T12:00:00.000Z"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `lat` | number | ✅ | Latitude |
| `lng` | number | ✅ | Longitude |
| `accuracy` | number | ✅ | GPS accuracy in meters |
| `timestamp` | string | ✅ | ISO 8601 timestamp |

**Response — `200 OK`** _(empty body)_

---

### 11.2 Get Partner Location

Get the current live location of the authenticated user's partner.

- **Method:** `GET`
- **Path:** `/api/location/partner`
- **Auth required:** Yes

**Response — `200 OK`** with `LocationData`, or **`404 Not Found`** if no partner or no location available.

---

## 12. Real-Time: Additional SignalR Events

### 12.1 Miss You Signal (Vibration)

Signal sent to trigger a vibration on the partner's device for approximately 1 minute along with a "I miss you" notification.

#### Client → Server: `MISS_YOU_SIGNAL`

```typescript
connection.invoke("MISS_YOU_SIGNAL", {
  to: string,       // partner's userId
  from: string,     // sender's username
  message: string   // e.g., "johndoe misses you 💕"
});
```

#### Server → Client: `MISS_YOU_SIGNAL`

```typescript
connection.on("MISS_YOU_SIGNAL", (data: {
  from: string;
  message: string;
}) => {
  // trigger vibration pattern (~1 minute)
  // show notification
});
```

---

### 12.2 Location Updates (Real-time)

For real-time partner location tracking.

#### Client → Server: `UPDATE_LOCATION`

```typescript
connection.invoke("UPDATE_LOCATION", {
  lat: number,
  lng: number,
  accuracy: number,
  timestamp: string
});
```

#### Server → Client: `PARTNER_LOCATION_UPDATE`

```typescript
connection.on("PARTNER_LOCATION_UPDATE", (data: LocationData) => {
  // update partner marker on map
});
```

---

### 12.3 SOS Alert (Real-time)

Notify the partner in-app when SOS is triggered.

#### Client → Server: `SOS_ALERT`

```typescript
connection.invoke("SOS_ALERT", {
  to: string,       // partner's userId
  location: { lat: number, lng: number },
  message: string
});
```

#### Server → Client: `SOS_ALERT`

```typescript
connection.on("SOS_ALERT", (data: {
  from: string;
  location: { lat: number; lng: number };
  message: string;
}) => {
  // show emergency alert with location
});
```

---

## 13. Emergency SOS — WhatsApp Integration

### 13.1 SOS Flow

When the user triggers Emergency SOS:

1. The client acquires GPS coordinates via the browser Geolocation API.
2. The client reads emergency contacts from the local database (up to 5 contacts).
3. For each emergency contact with a valid phone number, the client opens a WhatsApp deep link:
   `https://wa.me/{phone}?text={encodedSOSMessage}`
   The SOS message includes:
   - The user's custom emergency message
   - A Google Maps link to the user's live location
   - A "Sent from Vault R2 Safety Cloud" footer
4. The SOS is also sent via the SignalR hub (`SOS_ALERT` event) to the user's partner in-app.
5. The backend REST endpoint `POST /safety/trigger` can be used as a server-side fallback for SMS/push notification delivery via AWS SNS.

---

### 13.2 Trigger SOS (Enhanced)

- **Method:** `POST`
- **Path:** `/safety/trigger`
- **Auth required:** Yes

**Request Body**

```json
{
  "userId": "64b1f2c3d4e5f6a7b8c9d0e1",
  "location": { "lat": 51.5074, "lng": -0.1278 },
  "notifyPartner": true
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `userId` | string | ✅ | ID of the user triggering the SOS |
| `location` | `{ lat, lng }` | ✅ | Current GPS coordinates |
| `notifyPartner` | boolean | ❌ | If true, also notify the partner via in-app push |

**Response — `200 OK`** _(empty body)_

---

## 14. Error Responses

All REST endpoints use standard HTTP status codes. Error responses include a `message` field:

```json
{
  "message": "Detailed error description"
}
```

| Status Code | Meaning | Common Causes |
|---|---|---|
| `400 Bad Request` | Invalid or missing request parameters | Missing required fields, malformed JSON, duplicate email on register |
| `401 Unauthorized` | Authentication failure | Missing or expired JWT token, invalid credentials on login |
| `403 Forbidden` | Authorisation failure | Attempting to modify another user's data |
| `404 Not Found` | Resource not found | Invalid `friendId` in chat history, missing media item |
| `409 Conflict` | State conflict | Duplicate friend request, email already registered |
| `500 Internal Server Error` | Unhandled server error | Database connection failure, unexpected exception |

### Example error responses

**`400` — Missing required field**
```json
{
  "message": "The 'email' field is required."
}
```

**`401` — Expired token**
```json
{
  "message": "Your secure session has expired. Please log in again."
}
```

**`401` — Invalid credentials**
```json
{
  "message": "Invalid vault credentials."
}
```

**`409` — Duplicate registration**
```json
{
  "message": "Email already registered in this vault."
}
```
