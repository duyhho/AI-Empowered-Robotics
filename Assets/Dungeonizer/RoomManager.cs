using UnityEngine;
using System.Collections.Generic;
using System;

public class RoomManager : MonoBehaviour
{
    // List of all rooms.
    public List<Room> allRooms = new List<Room>();
    public List<Corridor> allCorridors = new List<Corridor>();

    public Vector3 startPoint;
    public Vector3 endPoint;
    // Prefabs and other properties can be added here.
    public GameObject roomColliderPrefab;
    public float parentOffsetHeight = 0f;
    public float tileScaling = 1f;
    // This method will generate a collider for a specific room.
    public void GenerateColliderForRoom(Room room)
    {
        if (roomColliderPrefab == null) return;

        // Assuming the room's x,y,w,h properties represent a rectangle's position and size
        Vector3 roomCenter = new Vector3(room.x + room.w / 2f, 0, room.y + room.h / 2f) + new Vector3(0f, parentOffsetHeight, 0f); // Adjust the Y value accordingly if needed.
        Vector3 roomSize = new Vector3(room.w, 1, room.h); // Adjust the Y value to your desired height.

        GameObject roomColliderObject = Instantiate(roomColliderPrefab, roomCenter, Quaternion.identity);
        BoxCollider roomCollider = roomColliderObject.GetComponent<BoxCollider>();

        if (roomCollider != null)
        {
            roomCollider.size = roomSize;
        }
    }
    public void GenerateCollidersForAllRooms(float tileScaling = 1f)
    {
        // Debug.Log("Generating for all rooms..." + allRooms.Count);
        // PrintAllRoomInfo();
        if (roomColliderPrefab == null) return;

        foreach (Room room in allRooms)
        {
            // Apply tileScaling to the entire calculation of room's center.
            Vector3 roomCenter = new Vector3((room.x + Mathf.FloorToInt(room.w / 2f)) * tileScaling, 0f, (room.y + Mathf.FloorToInt(room.h / 2f)) * tileScaling) + new Vector3(0f, parentOffsetHeight, 0f);

            Vector3 roomSize = new Vector3(room.w * tileScaling, 1, room.h * tileScaling); // Adjust the Y value to your desired height.

            GameObject roomColliderObject = Instantiate(roomColliderPrefab, roomCenter, Quaternion.identity);
            BoxCollider roomCollider = roomColliderObject.GetComponent<BoxCollider>();
            roomColliderObject.transform.parent = transform;
            if (roomCollider != null)
            {
                roomCollider.size = roomSize;
            }
        }
    }

    public void PrintAllRoomInfo()
    {
        // Print the number of rooms
        Debug.Log("Total number of rooms: " + allRooms.Count);

        // Iterate through each room and print its details
        int roomNumber = 1;
        foreach (Room room in allRooms)
        {
            Debug.Log($"Room {roomNumber}:");
            Debug.Log($"X: {room.x}, Y: {room.y}, Width: {room.w}, Height: {room.h}");
            roomNumber++;
        }
    }
    public void PrintAllCorridorInfo()
    {
        // Print the number of corridors
        Debug.Log("Total number of corridors: " + allCorridors.Count);

        // Iterate through each corridor and print its details
        int corridorNumber = 1;
        foreach (Corridor corridor in allCorridors)
        {
            Debug.Log($"Corridor {corridorNumber}:");
            Debug.Log($"Start X: {corridor.StartX}, Start Y: {corridor.StartY}, End X: {corridor.EndX}, End Y: {corridor.EndY}, Width: {corridor.Width}, Length: {corridor.Length}");
            corridorNumber++;
        }
    }

    // This method will generate a collider for a specific corridor.
    public void GenerateColliderForCorridor(Corridor corridor, float tileScaling = 1f)
    {
        if (roomColliderPrefab == null) return;

        Vector3 corridorCenter = new Vector3(0f, 0f, 0f);
        Vector3 corridorSize = new Vector3(0f, 0f, 0f);
        float centerOffset = corridor.Width % 2 == 0 ? -0.5f : 0;

        // If horizontal corridor
        if (corridor.StartY == corridor.EndY)
        {

            // Debug.Log($"Start X: {corridor.StartX}, Start Y: {corridor.StartY}, End X: {corridor.EndX}, End Y: {corridor.EndY}, Width: {corridor.Width}, Length: {corridor.Length}");
            // Debug.Log("Y Spawn: " + (corridor.StartY + corridor.Width / 2f) * tileScaling);
            corridorCenter = new Vector3((corridor.StartX + corridor.EndX) / 2f * tileScaling, parentOffsetHeight, (corridor.StartY + corridor.Width / 2f + centerOffset) * tileScaling);

            corridorSize = new Vector3(Mathf.Abs(corridor.EndX - corridor.StartX) * tileScaling, 1, corridor.Width * tileScaling);
        }
        else // Vertical corridor
        {
            corridorCenter = new Vector3((corridor.StartX + corridor.Width / 2f + centerOffset) * tileScaling, parentOffsetHeight, (corridor.StartY + corridor.EndY) / 2f * tileScaling);
            corridorSize = new Vector3(corridor.Width * tileScaling, 1, Mathf.Abs(corridor.EndY - corridor.StartY) * tileScaling);
        }

        GameObject corridorColliderObject = Instantiate(roomColliderPrefab, corridorCenter, Quaternion.identity);
        BoxCollider corridorCollider = corridorColliderObject.GetComponent<BoxCollider>();
        corridorColliderObject.transform.parent = transform;

        if (corridorCollider != null)
        {
            corridorCollider.size = corridorSize;
        }
    }

    public void GenerateCollidersForAllCorridors(float tileScaling = 1f)
    {
        // PrintAllCorridorInfo();
        foreach (Corridor corridor in allCorridors)
        {
            GenerateColliderForCorridor(corridor, tileScaling);
        }
    }

    // Add more methods and functionalities as needed.
    public void AddRoom(Room room)
    {
        allRooms.Add(room);
    }

    public Vector3 GetStartPoint()
    {
        return startPoint;
    }
    public Vector3 GetEndPoint()
    {
        return endPoint;
    }

    public Vector3 GetRandomGoalPosition()
    {
        // Debug.Log("AllRooms Length: " + allRooms.Count);

        if (allRooms.Count >= 1)
        {
            Room goalRoom = allRooms[allRooms.Count - 1];
            Room startRoom = allRooms[0]; // Note: You currently aren't using startRoom.

            // Generate a random offset within the room's boundaries.
            float xOffset = UnityEngine.Random.Range(0, goalRoom.w);
            float zOffset = UnityEngine.Random.Range(0, goalRoom.h);

            Vector3 endPointPosition = new Vector3((goalRoom.x + xOffset) * tileScaling, 0.5f + parentOffsetHeight, (goalRoom.y + zOffset) * tileScaling);
            return endPointPosition;
        }
        return endPoint;
    }

}
