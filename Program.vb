Imports System.ComponentModel.DataAnnotations
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports McMaster.Extensions.CommandLineUtils

Class MutliCastRelayer
    Public Shared Function Main(ByVal args As String()) As Integer
        Return CommandLineApplication.Execute(Of MutliCastRelayer)(args)
    End Function
    <Required>
    <[Option]("--multicast-address", ShortName:="m", Description:="Required. The from multicast address in the form of host:port. ")>
    Public Property MulticastAddress As String

    <Required>
    <[Option]("--destination-address", ShortName:="d", Description:="Required. The to address in the form of host:port. ")>
    Public Property DestinationAddress As String

    <[Option]("--bind-to", ShortName:="b", Description:="Optional. Bind to specific interface (All if not specified)")>
    Public Property BindToIp As String


    Sub OnExecute()
        Console.WriteLine("Multicast relay")
        Dim localIp As IPAddress = IPAddress.Any

        'check and parse params
        If Not String.IsNullOrWhiteSpace(BindToIp) Then
            If Not IPAddress.TryParse(BindToIp, localIp) Then
                ExitWithError("Bind to address is set, but cannot be parsed to an ip address.")
            End If
        End If

        Dim multicastEp = EndPointFromString(MulticastAddress)
        Dim relayEp = EndPointFromString(DestinationAddress)
        If multicastEp Is Nothing Then ExitWithError("Error parsing multicast address.")
        If relayEp Is Nothing Then ExitWithError("Error parsing unicast address.")
        Dim localEp As IPEndPoint = New IPEndPoint(localIp, multicastEp.Port)

        'setup th socket for multicast receive
        Dim client As UdpClient = New UdpClient With {
                .ExclusiveAddressUse = False
                }
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, True)
        client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, True)
        client.ExclusiveAddressUse = False
        client.Client.Bind(localEp)
        client.JoinMulticastGroup(multicastEp.Address)

        Console.WriteLine($"Started listening on {localEp} for multicast packets sent to {multicastEp.Address}")
        Console.WriteLine($"Packets will be relayed to: {relayEp}")
        Console.WriteLine($"{ Environment.NewLine}Press <ctrl-c> to quit")

        'create the forwarding client and prepare rest of the data used in infinite loop
        Dim forwardClient = New UdpClient(AddressFamily.InterNetwork)
        Dim receivEp As IPEndPoint = Nothing
        Dim packetsRelayed As Integer = 0
        While True
            'when data received, send it to the forward address and update the packets relayed
            Dim data As Byte() = client.Receive(receivEp)
            forwardClient.Send(data, data.Count, relayEp)
            packetsRelayed += 1
            'clean line
            Console.Write($"{ vbCr}                                     ")
            'and update our text
            Console.Write($"{ vbCr}Packets relayed: {packetsRelayed}")
        End While


    End Sub
    
    ''' <summary>
    ''' Returns IPEndPoint for a given string in the form of host:port
    ''' </summary>
    ''' <param name="endpointString"></param>
    ''' <returns>IPEndpoint when successfully parsed. Nothing on failure</returns>
    Private Function EndPointFromString(endpointString As String) As IPEndPoint
        Try
            Dim colonIndex = endpointString.IndexOf(":", StringComparison.Ordinal)
            Return New IPEndPoint(IPAddress.Parse(endpointString.Substring(0, colonIndex)), CType(endpointString.Substring(colonIndex + 1), Integer))
        Catch ex As Exception
            Return Nothing 'failed to parse
        End Try
    End Function

    Private Sub ExitWithError(errorMessage As String)
        'show message in red and exit with error
        Dim originalColor = Console.ForegroundColor
        Console.ForegroundColor=ConsoleColor.Red
        Console.WriteLine(errorMessage)
        console.ForegroundColor = originalColor
        Environment.Exit(-1)
    End Sub

End Class
