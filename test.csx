using System;
using System.Text.Json;

var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjNlZWVkYTU4LTU3NDktNDlhYi04NmRiLTc3YTBlYWRmMGFhYiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJ0ZXN0dXNlcjIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJVc2VyIiwiZXhwIjoxNzc3MjQyOTcyfQ.kY_Z0bV7Wn0yP0Q7hVbW-i8oJ5P7cT-R5Gj7LpWqT7g";
var payload = token.Split('.')[1];
var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
Console.WriteLine(json);
