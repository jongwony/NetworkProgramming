int isVictory(int *state){
	if(
		((state[0]&state[1]&state[2])&3) ||
		((state[3]&state[4]&state[5])&3) ||
		((state[6]&state[7]&state[8])&3) ||
		((state[0]&state[3]&state[6])&3) ||
		((state[1]&state[4]&state[7])&3) ||
		((state[2]&state[5]&state[8])&3) ||
		((state[0]&state[4]&state[8])&3) ||
		((state[2]&state[4]&state[6])&3)
	)	return 1;

	return 0;
}
