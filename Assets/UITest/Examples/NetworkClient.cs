using UnityEngine;
using System.Collections;
using System;

public class NetworkClient
{
    public string MockRequest;
    public string MockResponse = "Test answer";

    public string SendServerRequest(string request)
    {
        MockRequest = request;
        return MockResponse;
    }
}

