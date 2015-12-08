#ifndef __SFTP__

#define __SFTP__

#define RCVBUFSIZE 32
#define BUFSIZE 1024
#define FILESIZE 30
#define FILENAME 30
#define COMMANDSIZE 30
#define ENTRYSIZE 30

#define EchoReq 1
#define FileUpReq 2
#define FileDownReq 3
#define rlsReq 4
#define rcdReq 5
#define EchoAck 11
#define FileUpAck 12
#define FileDownAck 13
#define rlsAck 14
#define rcdAck 15

int servSock;			// socket descriptor for server
int clntSock;			// socket descriptor for client

char MsgType;
char fileName[FILENAME];	// buffer for file name
char fileSize[FILESIZE];	// buffer for file size
char fileBuf[BUFSIZE];		// buffer for file received by BUFSIZE
char echoBuffer[RCVBUFSIZE];	// buffer for echo string
char diritem[ENTRYSIZE];	// buffer for directory entry

#endif
