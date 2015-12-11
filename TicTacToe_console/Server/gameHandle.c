#include<sys/socket.h>

void gameHandle(int *clntSock){
	int flag[2] = {0,1};
	int turn=0;
	int coord;
	int isGameset;

	while(1){
		// broadcast turn
		send(clntSock[turn], &flag[0],4,0);
		send(clntSock[!turn], &flag[1],4,0);

		//coordinate 0-> 1
		recv(clntSock[turn],&coord,4,0);
		send(clntSock[!turn],&coord,4,0);

		// is Game set?
		recv(clntSock[turn], &isGameset,4,0);
		if(isGameset)	break;
		
		// alternative turn
		turn = !turn;
	}
}
